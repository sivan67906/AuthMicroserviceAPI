using AuthMicroservice.Application.Common;
using AuthMicroservice.Application.Features.Authentication.Login;
using AuthMicroservice.Domain.Entities;
using AuthMicroservice.Infrastructure.Data;
using AuthMicroservice.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AuthMicroservice.Application.Features.Authentication.RefreshToken;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken,
    string ClientId,
    string IpAddress) : IRequest<Result<LoginResponse>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly CommandDbContext _context;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        CommandDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _context = context;
        _configuration = configuration;
    }

    public async Task<Result<LoginResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return Result<LoginResponse>.Failure("Refresh token failed", "Invalid access token");
        }

        var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null || !Guid.TryParse(userId, out var userGuid))
        {
            return Result<LoginResponse>.Failure("Refresh token failed", "Invalid user in token");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
        {
            return Result<LoginResponse>.Failure("Refresh token failed", "User not found or inactive");
        }

        var storedRefreshToken = await _context.RefreshTokens
            .Include(rt => rt.Client)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken &&
                                      rt.UserId == userGuid &&
                                      rt.ClientId == request.ClientId,
                cancellationToken);

        if (storedRefreshToken == null)
        {
            return Result<LoginResponse>.Failure("Refresh token failed", "Invalid refresh token");
        }

        if (!storedRefreshToken.IsActive)
        {
            return Result<LoginResponse>.Failure("Refresh token failed", "Refresh token is not active");
        }

        if (storedRefreshToken.IsExpired)
        {
            return Result<LoginResponse>.Failure("Refresh token failed", "Refresh token has expired");
        }

        // Revoke old refresh token
        storedRefreshToken.RevokedAt = DateTime.UtcNow;
        storedRefreshToken.RevokedByIp = request.IpAddress;

        // Generate new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        // Store new refresh token - Note: Fully qualified type name to avoid namespace collision
        var newRefreshTokenEntity = new AuthMicroservice.Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshToken,
            ClientId = request.ClientId,
            UserAgent = storedRefreshToken.UserAgent,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = request.IpAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(
                double.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7"))
        };

        storedRefreshToken.ReplacedByToken = newRefreshToken;

        _context.RefreshTokens.Add(newRefreshTokenEntity);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15")),
            newRefreshTokenEntity.ExpiresAt,
            new UserInfo(user.Id, user.Email!, user.FullName, roles)
        );

        return Result<LoginResponse>.Success(response, "Token refreshed successfully");
    }
}
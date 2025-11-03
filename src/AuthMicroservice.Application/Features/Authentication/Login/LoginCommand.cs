using AuthMicroservice.Application.Common;
using AuthMicroservice.Domain.Entities;
using AuthMicroservice.Infrastructure.Data;
using AuthMicroservice.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthMicroservice.Application.Features.Authentication.Login;

public record LoginCommand(
    string Email,
    string Password,
    string ClientId,
    string UserAgent,
    string IpAddress) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpires,
    DateTime RefreshTokenExpires,
    UserInfo User);

public record UserInfo(
    Guid Id,
    string Email,
    string? FullName,
    IList<string> Roles);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly CommandDbContext _context;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        CommandDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _context = context;
        _configuration = configuration;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result<LoginResponse>.Failure("Login failed", "Invalid credentials");
        }

        if (!user.IsActive)
        {
            return Result<LoginResponse>.Failure("Login failed", "Account is deactivated");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return Result<LoginResponse>.Failure("Login failed", "Account is locked out");
            }
            if (result.IsNotAllowed)
            {
                return Result<LoginResponse>.Failure("Login failed", "Login not allowed");
            }
            return Result<LoginResponse>.Failure("Login failed", "Invalid credentials");
        }

        // Verify client exists
        var client = await _context.Clients.FindAsync(request.ClientId);
        if (client == null || !client.IsActive)
        {
            return Result<LoginResponse>.Failure("Login failed", "Invalid client");
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ClientId = request.ClientId,
            UserAgent = request.UserAgent,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = request.IpAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(
                double.Parse(_configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7"))
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15")),
            refreshTokenEntity.ExpiresAt,
            new UserInfo(user.Id, user.Email!, user.FullName, roles)
        );

        return Result<LoginResponse>.Success(response, "Login successful");
    }
}

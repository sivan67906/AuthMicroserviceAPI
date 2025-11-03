using AuthMicroservice.Application.Common;
using AuthMicroservice.Domain.Entities;
using AuthMicroservice.Infrastructure.Data;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AuthMicroservice.Application.Features.Authentication.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string ConfirmPassword,
    string? FullName) : IRequest<Result<RegisterResponse>>;

public record RegisterResponse(
    Guid Id,
    string Email,
    string? FullName,
    DateTime CreatedAt);

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number")
            .Matches(@"[@$!%*?&#]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match");

        RuleFor(x => x.FullName)
            .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly CommandDbContext _context;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        CommandDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<RegisterResponse>.Failure(
                "Registration failed",
                "A user with this email already exists");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            IsActive = true,
            IsEmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return Result<RegisterResponse>.Failure("Registration failed", errors);
        }

        // Assign default User role
        await _userManager.AddToRoleAsync(user, "User");

        // Sync to read database
        await SyncToReadDatabase(user);

        var response = user.Adapt<RegisterResponse>();
        return Result<RegisterResponse>.Success(response, "User registered successfully");
    }

    private async Task SyncToReadDatabase(ApplicationUser user)
    {
        // In a real application, this would be done via event sourcing or message bus
        // For now, we'll just log it
        await Task.CompletedTask;
    }
}

using AuthMicroservice.Application.Common;
using AuthMicroservice.Application.Features.Authentication.Login;
using AuthMicroservice.Application.Features.Authentication.RefreshToken;
using AuthMicroservice.Application.Features.Authentication.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthMicroservice.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<RegisterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", command.Email);
        
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse.FailResponse(result.Message!, result.Errors));
        }

        return Ok(ApiResponse<RegisterResponse>.SuccessResponse(result.Data!, result.Message));
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);
        
        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.ClientId,
            Request.Headers.UserAgent.ToString(),
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        );

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse.FailResponse(result.Message!, result.Errors));
        }

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(result.Data!, result.Message));
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(
            request.AccessToken,
            request.RefreshToken,
            request.ClientId,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
        );

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse.FailResponse(result.Message!, result.Errors));
        }

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(result.Data!, result.Message));
    }

    /// <summary>
    /// Test endpoint for simple authorization
    /// </summary>
    [Authorize]
    [HttpGet("test-auth")]
    public IActionResult TestAuth()
    {
        return Ok(new { Message = "Simple Authorization works!", User = User.Identity?.Name });
    }

    /// <summary>
    /// Test endpoint for role-based authorization
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("test-admin")]
    public IActionResult TestAdmin()
    {
        return Ok(new { Message = "Role-Based Authorization works! You are an Admin." });
    }

    /// <summary>
    /// Test endpoint for claims-based authorization
    /// </summary>
    [Authorize(Policy = "RequireFullAccessClaim")]
    [HttpGet("test-claims")]
    public IActionResult TestClaims()
    {
        return Ok(new { Message = "Claims-Based Authorization works! You have FullAccess claim." });
    }

    /// <summary>
    /// Test endpoint for policy-based authorization
    /// </summary>
    [Authorize(Policy = "RequireAdminOrModerator")]
    [HttpGet("test-policy")]
    public IActionResult TestPolicy()
    {
        return Ok(new { Message = "Policy-Based Authorization works! You are Admin or Moderator." });
    }
}

public record LoginRequest(string Email, string Password, string ClientId);
public record RefreshTokenRequest(string AccessToken, string RefreshToken, string ClientId);

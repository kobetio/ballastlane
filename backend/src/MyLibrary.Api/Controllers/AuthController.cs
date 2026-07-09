using Microsoft.AspNetCore.Mvc;
using MyLibrary.Application.DTOs.Auth;
using MyLibrary.Application.Interfaces;

namespace MyLibrary.Api.Controllers;

/// <summary>
/// Registration and login endpoints. Both return a JWT to be sent as
/// <c>Authorization: Bearer {token}</c> on subsequent requests.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Creates a new user account.</summary>
    /// <param name="request">Name, email and password of the new user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">The account was created; returns a JWT and the user's profile.</response>
    /// <response code="400">The request failed validation, or the email is already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>Authenticates an existing user.</summary>
    /// <param name="request">Email and password of the account.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Credentials were valid; returns a JWT and the user's profile.</response>
    /// <response code="400">The request failed validation.</response>
    /// <response code="401">The email or password is incorrect.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }
}

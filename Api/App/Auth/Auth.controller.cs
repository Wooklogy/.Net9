using Api.App.Auth.Dto;
using Api.App.Auth.Interfaces;
using Api.Config.Error;
using Microsoft.AspNetCore.Mvc;

namespace Api.App.Auth;

[ApiController]
[Route("auth")]
[Tags("Authentication & Authorization")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("sign-up")]
    [EndpointSummary("Create User")]
    [EndpointDescription("Sign up user account for long-term semiconductor investment system.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesErrorCodes(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SignUp([FromBody] PostSignUpQTO qto)
    {
        await authService.SignUp(qto);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    [EndpointSummary("Login")]
    [EndpointDescription("Sign in to the system. Returns a JWT token for secure access.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesErrorCodes(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] GetLoginQTO qto)
    {
        var token = await authService.Login(qto);
        return Ok(token);
    }
}
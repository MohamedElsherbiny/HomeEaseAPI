using HomeEase.Application.Commands.AuthCommands;
using HomeEase.Application.DTOs;
using HomeEase.Resources;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Authentication;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator _mediator) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
    {
        try
        {
            var command = new LoginCommand(loginRequest);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (AuthenticationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
    {
        try
        {
            var command = new RegisterUserCommand(registerDto);
            var result = await _mediator.Send(command);
            return Created(string.Empty, result);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("register-provider")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterProvider([FromBody] RegisterProviderCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Created(string.Empty, result);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] PasswordResetRequestDto requestDto)
    {
        return Ok(await _mediator.Send(new RequestPasswordResetCommand(requestDto)));
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto resetDto)
    {
        return Ok(await _mediator.Send(new ResetPasswordCommand(resetDto)));
    }

    [HttpPost("verify-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                IsValid = false,
                Message = Messages.InvalidOrExpiredOtp,
            });
        }

        return Ok(new
        {
            IsValid = true,
            Message = Messages.ValidOtp,
        });
    }

    [HttpPost("v2/verify-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtpV2([FromBody] VerifyOtpCommand command)
    {
        return Ok(await _mediator.Send(command));
    }
}

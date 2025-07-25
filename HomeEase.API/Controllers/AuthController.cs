﻿using HomeEase.Application.Commands.AuthCommands;
using HomeEase.Application.DTOs;
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
        var command = new RequestPasswordResetCommand(requestDto);
        await _mediator.Send(command);
        return Ok(new { message = "If your email exists in our system, you will receive a password reset link." });
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDto resetDto)
    {
        try
        {
            var command = new ResetPasswordCommand(resetDto);
            await _mediator.Send(command);
            return Ok(new { message = "Password has been reset successfully." });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
    {
        var result = await _mediator.Send(command);

        return result
            ? Ok(new
            {
                IsValid = true,
                Message = "OTP code is valid",
            })
            : BadRequest(new
            {
                IsValid = false,
                Message = "Invalid or expired OTP code",
            });
    }

    //[HttpPost("refresh-token")]
    //[ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
    //{
    //    try
    //    {
    //        var command = new RefreshTokenCommand(refreshTokenDto);
    //        var result = await _mediator.Send(command);
    //        return Ok(result);
    //    }
    //    catch (SecurityTokenException)
    //    {
    //        return Unauthorized(new { message = "Invalid token" });
    //    }
    //}
}

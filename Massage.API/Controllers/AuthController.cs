using Massage.Application.Commands;
using Massage.Application.Commands.AuthCommands;
using Massage.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Authentication;

namespace Massage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

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
        public async Task<IActionResult> RegisterProvider([FromBody] RegisterProviderDto registerDto)
        {
            try
            {
                var command = new RegisterProviderCommand(registerDto);
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

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var command = new RefreshTokenCommand(refreshTokenDto);
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (SecurityTokenException)
            {
                return Unauthorized(new { message = "Invalid token" });
            }
        }
    }
}

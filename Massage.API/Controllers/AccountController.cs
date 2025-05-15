using Massage.Application.Commands;
using Massage.Application.DTOs;
using Massage.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Massage.Application.Exceptions;
using Massage.Application.Interfaces.Services;
using Massage.Application.Commands.UserCommends;
using Massage.Application.Queries.UserQueries;
using Massage.Infrastructure.Services;

namespace Massage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public AccountController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = _currentUserService.UserId;
            var query = new GetUserByIdQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDto updateDto)
        {
            var userId = _currentUserService.UserId;
            var command = new UpdateUserCommand(userId, updateDto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var userId = _currentUserService.UserId;
            var command = new ChangePasswordCommand(userId, changePasswordDto);

            try
            {
                await _mediator.Send(command);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("preferences")]
        [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserPreferences()
        {
            var userId = _currentUserService.UserId;
            var query = new GetUserPreferencesQuery(userId);

            try
            {
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("preferences")]
        [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateUserPreferences([FromBody] UserPreferencesDto preferencesDto)
        {
            // Ensure the user can only update their own preferences
            var userId = _currentUserService.UserId;
            preferencesDto.UserId = userId;

            var command = new UpdateUserPreferencesCommand(preferencesDto);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("addresses")]
        [ProducesResponseType(typeof(IEnumerable<AddressDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserAddresses()
        {
            var userId = _currentUserService.UserId;
            var query = new GetUserAddressesQuery(userId);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("addresses")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddUserAddress([FromBody] AddressDto addressDto)
        {
            var userId = _currentUserService.UserId;
            var command = new AddUserAddressCommand(userId, addressDto);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetUserAddresses), null, result);
        }

        [HttpPut("addresses/{addressId}")]
        [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserAddress(Guid addressId, [FromBody] AddressDto addressDto)
        {
            var userId = _currentUserService.UserId;
            var command = new UpdateUserAddressCommand(userId, addressId, addressDto);

            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("addresses/{addressId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUserAddress(Guid addressId)
        {
            var userId = _currentUserService.UserId;
            var command = new DeleteUserAddressCommand(userId, addressId);

            try
            {
                await _mediator.Send(command);
                return Ok(new { message = "Address deleted successfully" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpPost("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAccountStatus()
        {
            var userId = _currentUserService.UserId;
            var command = new GetUserStatusCommand(userId);
            var status = await _mediator.Send(command);

            return Ok(new
            {
                message = "Account status retrieved successfully",
                status
            });
        }
    }
}

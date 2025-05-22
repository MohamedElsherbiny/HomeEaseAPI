using HomeEase.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Commands.UserCommends;
using HomeEase.Application.Queries.UserQueries;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController(IMediator _mediator, ICurrentUserService _currentUserService) : ControllerBase
{

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _mediator.Send(new GetUserByIdQuery(_currentUserService.UserId));

        return Ok(result);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDto updateDto)
    {
        var result = await _mediator.Send(new UpdateUserCommand(_currentUserService.UserId, updateDto));

        return Ok(result);
    }

    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        await _mediator.Send(new ChangePasswordCommand(_currentUserService.UserId, changePasswordDto));

        return Ok(new { message = "Password changed successfully" });
    }

    [HttpGet("preferences")]
    [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserPreferences()
    {
        var userId = _currentUserService.UserId;
        var query = new GetUserPreferencesQuery(userId);

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("preferences")]
    [ProducesResponseType(typeof(UserPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUserPreferences([FromBody] UserPreferencesDto preferencesDto)
    {
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
        var result = await _mediator.Send(new UpdateUserAddressCommand(_currentUserService.UserId, addressId, addressDto));

        return Ok(result);
    }

    [HttpDelete("addresses/{addressId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUserAddress(Guid addressId)
    {
        var userId = _currentUserService.UserId;
        var command = new DeleteUserAddressCommand(userId, addressId);


        await _mediator.Send(command);
        return Ok(new { message = "Address deleted successfully" });
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

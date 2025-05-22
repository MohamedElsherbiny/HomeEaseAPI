using HomeEase.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HomeEase.Application.Commands.UserCommends;
using HomeEase.Application.Queries.UserQueries;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class UsersController(IMediator _mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(userId));
        return Ok(result);
    }

    [HttpPost("activate/{id}")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var command = new ActivateUserCommand(id);
        var result = await _mediator.Send(command);
        return result ? Ok("User activation successful.") : BadRequest("Failed to activate user.");
    }

    [HttpPost("deactivate/{id}")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var command = new DeactivateUserCommand(id);
        var result = await _mediator.Send(command);
        return result ? Ok("User deactivation successful.") : BadRequest("Failed to deactivate user.");
    }

}

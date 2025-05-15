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
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public UsersController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{userId}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var query = new GetUserByIdQuery(userId);
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


        [HttpPost("deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeactivateAccount()
        {
            var userId = _currentUserService.UserId;
            var command = new DeactivateUserCommand(userId);
            await _mediator.Send(command);
            return Ok(new { message = "Account deactivated successfully" });
        }


        [HttpPost("activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ActivateAccount()
        {
            var userId = _currentUserService.UserId;
            var command = new ActivateUserCommand(userId);
            await _mediator.Send(command);
            return Ok(new { message = "Account activated successfully" });
        }

    }
}

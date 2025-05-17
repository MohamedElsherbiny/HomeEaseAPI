using Massage.Application.Commands.AdminCommands;
using Massage.Application.Queries.AdminQueries;
using Massage.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Massage.API.Controllers
{
    [ApiController]
    [Route("api/admins")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. Create new admin
        [HttpPost("create")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // 2. Delete admin by ID
        [HttpDelete("{adminId}")]
        public async Task<IActionResult> DeleteAdmin(Guid adminId)
        {
            var result = await _mediator.Send(new DeleteAdminCommand(adminId));
            return result ? Ok("Admin deleted successfully.") : BadRequest("Failed to delete admin.");
        }

        // 3. Get all admins
        [HttpGet("list")]
        public async Task<IActionResult> GetAllAdmins()
        {
            var result = await _mediator.Send(new GetAllAdminsQuery());
            return Ok(result);
        }
    }
}
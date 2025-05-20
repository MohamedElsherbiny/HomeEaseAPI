using Massage.Application.Commands.AdminCommands;
using Massage.Application.Queries.AdminQueries;
using Massage.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Massage.Application.Commands.UserCommends;
using Massage.Application.Commands.ProviderCommands;

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

        // 3. Get all admins with pagination
        [HttpGet("list")]
        public async Task<IActionResult> GetAllAdmins(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string sortBy = "CreatedAt",
            [FromQuery] bool sortDescending = true,
            [FromQuery] bool? isActive = null)
        {
            var query = new GetAllAdminsQuery
            {
                Page = page,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending,
                IsActive = isActive
            };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
using Massage.Application.Commands.AdminCommands;
using Massage.Application.Queries.AdminQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Massage.API.Controllers;

[ApiController]
[Route("api/admins")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(IMediator _mediator) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{adminId}")]
    public async Task<IActionResult> DeleteAdmin(Guid adminId)
    {
        var result = await _mediator.Send(new DeleteAdminCommand(adminId));
        return result ? Ok("Admin deleted successfully.") : BadRequest("Failed to delete admin.");
    }

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
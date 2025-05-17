using Massage.Application.Commands.AdminCommands;
using Massage.Application.Queries.AdminQueries;
using Massage.Application.Queries.BookingQueries;
using Massage.Application.Queries.ProviderQueries;
using Massage.Application.Queries.UserQueries;
using Massage.Application.DTOs;
using Massage.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Massage.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Admin Management Endpoints

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

        // Report Endpoints

        // 4. Get booking statistics
        [HttpGet("bookings/statistics")]
        public async Task<ActionResult> GetBookingStatistics()
        {
            var query = new GetBookingStatisticsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // 5. Get dashboard stats
        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboardStats()
        {
            var query = new GetDashboardStatsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // 6. Get booking reports
        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<AdminBookingReportDto>>> GetBookingReports(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] BookingStatus? status)
        {
            var query = new GetBookingReportsQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                Status = status
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // 7. Get provider reports
        [HttpGet("providers")]
        public async Task<ActionResult<IEnumerable<AdminProviderReportDto>>> GetProviderReports(
            [FromQuery] ProviderStatus? status,
            [FromQuery] bool? isActive)
        {
            var query = new GetProviderReportsQuery
            {
                Status = status,
                IsActive = isActive
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // 8. Get user reports
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<AdminUserReportDto>>> GetUserReports(
            [FromQuery] UserRole? role,
            [FromQuery] bool? isActive)
        {
            var query = new GetUserReportsQuery
            {
                Role = role,
                IsActive = isActive
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // 9. Get platform stats
        [HttpGet("platform-stats")]
        public async Task<ActionResult<PlatformStatsDto>> GetPlatformStats()
        {
            var query = new GetPlatformStatsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
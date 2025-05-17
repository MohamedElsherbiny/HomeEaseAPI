using Massage.Application.Queries.BookingQueries;
using Massage.Application.Queries.ProviderQueries;
using Massage.Application.Queries.UserQueries;
using Massage.Application.DTOs;
using Massage.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Massage.Application.Queries.AdminQueries;

namespace Massage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // 1. Get booking statistics
        [HttpGet("bookings/statistics")]
        public async Task<ActionResult> GetBookingStatistics()
        {
            var query = new GetBookingStatisticsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // 2. Get dashboard stats
        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboardStats()
        {
            var query = new GetDashboardStatsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // 3. Get booking reports
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

        // 4. Get provider reports
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

        // 5. Get user reports
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

        // 6. Get platform stats
        [HttpGet("platform-stats")]
        public async Task<ActionResult<PlatformStatsDto>> GetPlatformStats()
        {
            var query = new GetPlatformStatsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
using HomeEase.Application.Queries.BookingQueries;
using HomeEase.Application.DTOs;
using HomeEase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HomeEase.Application.Queries.AdminQueries;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ReportsController(IMediator _mediator) : ControllerBase
{
    [HttpGet("bookings/statistics")]
    public async Task<ActionResult> GetBookingStatistics()
    {
        var query = new GetBookingStatisticsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboardStats()
    {
        var query = new GetDashboardStatsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

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

    [HttpGet("platform-stats")]
    public async Task<ActionResult<PlatformStatsDto>> GetPlatformStats()
    {
        var query = new GetPlatformStatsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
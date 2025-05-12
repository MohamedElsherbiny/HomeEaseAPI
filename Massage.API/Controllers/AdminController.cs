using Massage.Application.Commands.AdminCommands;
using Massage.Application.Commands.ProviderCommands;
using Massage.Application.Queries.AdminQueries;
using Massage.Application.Queries.BookingQueries;
using Massage.Application.Queries.ProviderQueries;
using Massage.Application.Queries.UserQueries;
using Massage.Application.DTOs;
using Massage.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Massage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
        {
            var query = new GetAllUsersQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("providers")]
        public async Task<ActionResult<IEnumerable<ProviderDto>>> GetAllProviders()
        {
            var query = new GetAllProvidersQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("bookings/statistics")]
        public async Task<ActionResult> GetBookingStatistics()
        {
            var query = new GetBookingStatisticsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("providers/{providerId}/verify")]
        public async Task<ActionResult> VerifyProvider(Guid providerId)
        {
            var command = new VerifyProviderCommand { ProviderId = providerId };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("providers/{providerId}")]
        public async Task<ActionResult> DeleteProvider(Guid providerId)
        {
            var command = new DeleteProviderCommand { ProviderId = providerId };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboardStats()
        {
            var query = new GetDashboardStatsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("users/filtered")]
        public async Task<ActionResult<List<UserDto>>> GetFilteredUsers(
            [FromQuery] string? searchTerm,
            [FromQuery] UserRole? role,
            [FromQuery] bool? isActive,
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetFilteredUsersQuery
            {
                SearchTerm = searchTerm,
                Role = role,
                IsActive = isActive,
                FromDate = fromDate,
                ToDate = toDate,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("users/{userId}/block")]
        public async Task<ActionResult> BlockUser(Guid userId)
        {
            var command = new BlockUserCommand { UserId = userId };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPut("users/{userId}/unblock")]
        public async Task<ActionResult> UnblockUser(Guid userId)
        {
            var command = new UnblockUserCommand { UserId = userId };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPut("users/{userId}/change-role")]
        public async Task<ActionResult> ChangeUserRole(Guid userId, [FromBody] UserRole newRole)
        {
            var command = new ChangeUserRoleCommand
            {
                UserId = userId,
                NewRole = newRole
            };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("reports/bookings")]
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

        [HttpGet("reports/providers")]
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

        [HttpGet("reports/users")]
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
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Massage.Application.Commands;
using Massage.Application.Queries;
using Massage.Application.DTOs;
using System.Security.Claims;
using Massage.Application.Commands.ProviderCommands;
using Massage.Application.Queries.ProviderQueries;
using Massage.Domain.Common;

namespace Massage.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProvidersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProvidersController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PaginatedList<ProviderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllProvidersAsync([FromQuery] GetAllProvidersQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ProviderDto>> GetProviderById(Guid id)
        {
            var query = new GetProviderByIdQuery { ProviderId = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("user")]
        public async Task<ActionResult<ProviderDto>> GetProviderByCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guidUserId))
                return BadRequest("Invalid user ID");

            var query = new GetProviderByUserIdQuery { UserId = guidUserId };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ProviderSearchResultDto>>> SearchProviders(
            [FromQuery] double? latitude,
            [FromQuery] double? longitude,
            [FromQuery] double? maxDistance,
            [FromQuery] string[] serviceTypes,
            [FromQuery] decimal? minRating,
            [FromQuery] string city,
            [FromQuery] string state,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new SearchProvidersQuery
            {
                Latitude = latitude,
                Longitude = longitude,
                MaxDistance = maxDistance,
                ServiceTypes = serviceTypes,
                MinRating = minRating,
                City = city,
                State = state,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "ProviderOnly")]
        public async Task<IActionResult> UpdateProvider(Guid id, UpdateProviderDto updateDto)
        {
            var command = new UpdateProviderCommand { ProviderId = id, ProviderDto = updateDto };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteProvider(Guid id)
        {
            var command = new DeleteProviderCommand { ProviderId = id };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id}/verify")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> VerifyProvider(Guid id)
        {
            var command = new VerifyProviderCommand { ProviderId = id };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/schedule")]
        [Authorize(Policy = "ProviderOnly")]
        public async Task<IActionResult> UpdateProviderSchedule(Guid id, ProviderScheduleDto scheduleDto)
        {
            var command = new UpdateProviderScheduleCommand { ProviderId = id, ScheduleDto = scheduleDto };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }

    }
}

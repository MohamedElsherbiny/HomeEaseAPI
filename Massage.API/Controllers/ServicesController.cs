using Massage.Application.Commands.ServiceCommands;
using Massage.Application.DTOs;
using Massage.Application.Queries.ServiceQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Massage.API.Controllers
{
    [ApiController]
    [Route("api/providers/{providerId}/services")]
    //[Authorize]
    public class ProviderServicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProviderServicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<ServiceDto>>> GetServicesByProvider(Guid providerId)
        {
            var query = new GetServicesByProviderQuery { ProviderId = providerId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{serviceId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceDto>> GetServiceById(Guid providerId, Guid serviceId)
        {
            var query = new GetServiceByIdQuery { ServiceId = serviceId };
            var result = await _mediator.Send(query);

            if (result == null || result.Id != serviceId)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        //[Authorize(Roles = "Provider")]
        public async Task<ActionResult<Guid>> CreateService(Guid providerId, CreateServiceDto serviceDto)
        {
            var command = new CreateServiceCommand { ProviderId = providerId, ServiceDto = serviceDto };
            var serviceId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetServiceById), new { providerId, serviceId }, serviceId);
        }

        [HttpPut("{serviceId}")]
        //[Authorize(Roles = "Provider")]
        public async Task<IActionResult> UpdateService(Guid providerId, Guid serviceId, UpdateServiceDto serviceDto)
        {
            var command = new UpdateServiceCommand { ServiceId = serviceId, ServiceDto = serviceDto };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{serviceId}")]
        //[Authorize(Roles = "Provider")]
        public async Task<IActionResult> DeleteService(Guid providerId, Guid serviceId)
        {
            var command = new DeleteServiceCommand { ServiceId = serviceId };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}

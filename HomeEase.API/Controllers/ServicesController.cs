using HomeEase.Application.Commands.ServiceCommands;
using HomeEase.Application.DTOs.ProviderService;
using HomeEase.Application.Queries.ServiceQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/providers/services/{providerId}")]
[Authorize]
public class ProviderServicesController(IMediator _mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ServiceDto>>> GetServicesByProvider(Guid providerId)
    {
        var result = await _mediator.Send(new GetServicesByProviderQuery { ProviderId = providerId });

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
    [Authorize(Policy = "ProviderOnly")]
    public async Task<ActionResult<Guid>> CreateService(Guid providerId, CreateServiceDto serviceDto)
    {
        return Ok(await _mediator.Send(new CreateServiceCommand { ProviderId = providerId, ServiceDto = serviceDto }));
    }

    [HttpPost("bulk")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<ActionResult<List<Guid>>> CreateServices(Guid providerId, BlukUpdateServicesDto servicesDto)
    {
        return Ok(await _mediator.Send(new CreateServicesCommand { ProviderId = providerId, ServicesDto = servicesDto }));
    }

    [HttpPut("{serviceId}")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<IActionResult> UpdateService(Guid providerId, Guid serviceId, UpdateServiceDto serviceDto)
    {
        return Ok(await _mediator.Send(new UpdateServiceCommand { ServiceId = serviceId, ServiceDto = serviceDto }));
    }

    [HttpDelete("{serviceId}")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<IActionResult> DeleteService(Guid providerId, Guid serviceId)
    {
        return Ok(await _mediator.Send(new DeleteServiceCommand { ServiceId = serviceId }));
    }

    [HttpPatch("activate/{serviceId}")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<ActionResult<ServiceDto>> ActivateService(Guid providerId, Guid serviceId)
    {

        var command = new ActivateServiceCommand { ProviderId = providerId, ServiceId = serviceId };
        var result = await _mediator.Send(command);
        return Ok(result);

    }

    [HttpPatch("deactivate/{serviceId}")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<ActionResult<ServiceDto>> DeactivateService(Guid providerId, Guid serviceId)
    {
        var command = new DeactivateServiceCommand { ProviderId = providerId, ServiceId = serviceId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

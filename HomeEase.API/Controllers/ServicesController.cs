﻿using HomeEase.Application.Commands.ServiceCommands;
using HomeEase.Application.DTOs;
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
        var command = new CreateServiceCommand { ProviderId = providerId, ServiceDto = serviceDto };
        var serviceId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetServiceById), new { providerId, serviceId }, serviceId);
    }

    [HttpPost("bulk")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<ActionResult<List<Guid>>> CreateServices(Guid providerId, CreateServicesDto servicesDto)
    {
        var command = new CreateServicesCommand { ProviderId = providerId, ServicesDto = servicesDto };
        await _mediator.Send(command);

        return Ok();
    }

    [HttpPut("{serviceId}")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<IActionResult> UpdateService(Guid providerId, Guid serviceId, UpdateServiceDto serviceDto)
    {
        var command = new UpdateServiceCommand { ServiceId = serviceId, ServiceDto = serviceDto };
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{serviceId}")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<IActionResult> DeleteService(Guid providerId, Guid serviceId)
    {
        var command = new DeleteServiceCommand { ServiceId = serviceId };
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("activate/{serviceId}")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<ActionResult<ServiceDto>> ActivateService(Guid providerId, Guid serviceId)
    {
        try
        {
            var command = new ActivateServiceCommand { ProviderId = providerId, ServiceId = serviceId };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("deactivate/{serviceId}")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<ActionResult<ServiceDto>> DeactivateService(Guid providerId, Guid serviceId)
    {
        try
        {
            var command = new DeactivateServiceCommand { ProviderId = providerId, ServiceId = serviceId };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

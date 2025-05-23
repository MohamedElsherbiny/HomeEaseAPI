using HomeEase.Application.Commands.PlatformService;
using HomeEase.Application.DTOs;
using HomeEase.Application.Queries.PlatformService;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeEase.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class PlatformServicesController(IMediator _mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreatePlatformServiceCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPlatformServices([FromQuery] GetAllPlatformServicesQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPlatformServiceByIdQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdatePlatformServiceDto dto)
    {
        var success = await _mediator.Send(new UpdatePlatformServiceCommand
        {
            Id = id,
            Name = dto.Name,
            ImageUrl = dto.ImageUrl
        });
        if (!success) return NotFound();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeletePlatformServiceCommand { Id = id });
        if (!success) return NotFound();
        return Ok();
    }

    [HttpPatch("activate/{id}")]
    public async Task<IActionResult> ActivateService(Guid id)
    {
        var command = new ActivatePlatformServiceCommand { Id = id };
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPatch("deactivate/{id}")]
    public async Task<IActionResult> DeactivateService(Guid id)
    {
        var result = await _mediator.Send(new DeactivatePlatformServiceCommand { Id = id });

        return Ok(result);
    }
}

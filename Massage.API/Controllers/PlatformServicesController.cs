using Massage.Application.Commands;
using Massage.Application.Commands.PlatformService;
using Massage.Application.DTOs;
using Massage.Application.Exceptions;
using Massage.Application.Queries.PlatformService;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Massage.API.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("api/[controller]")]
    public class PlatformServicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PlatformServicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

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

        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateService(Guid id)
        {
            try
            {
                var command = new ActivatePlatformServiceCommand { Id = id };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateService(Guid id)
        {
            try
            {
                var command = new DeactivatePlatformServiceCommand { Id = id };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}

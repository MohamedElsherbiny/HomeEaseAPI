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

        [HttpPost("{id}/Platform-service-image")]
        public async Task<ActionResult<UpdateBasePlatformServiceImageResponse>> UpdatePlatformServiceImage(Guid id, IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image file provided");
            }

            // Check file size (max 5MB)
            if (image.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size exceeds the limit (5MB)");
            }

            // Check file type
            var extension = Path.GetExtension(image.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                return BadRequest("Only JPG, JPEG, and PNG files are allowed");
            }

            try
            {
                var command = new UpdateBasePlatformServiceImageCommand(id, image);
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}

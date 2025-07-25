﻿using HomeEase.Application.Commands.ProviderCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Queries.ProviderQueries;
using HomeEase.Domain.Common;
using HomeEase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProvidersController(IMediator _mediator, IWebHostEnvironment _webHostEnvironment, IDataExportService _exportService) : ControllerBase
{
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

    [HttpPost("verify/{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> VerifyProvider(Guid id)
    {
        var command = new VerifyProviderCommand { ProviderId = id };
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("schedule/{id}")]
    [Authorize(Policy = "ProviderOnly")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateProviderSchedule(Guid id, ProviderScheduleDto scheduleDto)
    {
        var command = new UpdateProviderScheduleCommand { ProviderId = id, ScheduleDto = scheduleDto };
        var result = await _mediator.Send(command);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPatch("activate/{id}")]
    public async Task<IActionResult> ActivateProvider(Guid id)
    {
        var command = new ActivateProviderCommand(id);
        var result = await _mediator.Send(command);
        return result ? Ok("Provider activation successful.") : BadRequest("Failed to activate provider.");
    }


    [HttpPatch("deactivate/{id}")]
    public async Task<IActionResult> DeactivateProvider(Guid id)
    {
        var command = new DeactivateProviderCommand(id);
        var result = await _mediator.Send(command);
        return result ? Ok("Provider deactivation successful.") : BadRequest("Failed to deactivate provider.");
    }

    [HttpGet("Export")]
    public async Task<IActionResult> Export([FromQuery] GetAllProvidersQuery query)
    {
        var providers = await _mediator.Send(query);

        var request = new ExportRequest<ProviderDto>
        {
            Data = providers.Items,
            ExportFormat = query.ExportFormat,
            TemplatePath = Path.Combine(_webHostEnvironment.WebRootPath, "ExporTemplates", "Providers"),
            ColumnMappings = new Dictionary<string, Func<ProviderDto, string>>
            {
                ["{FirstName}"] = p => p.User.FirstName,
                ["{LastName}"] = p => p.User.LastName,
                ["{Email}"] = p => p.User.Email,
                ["{PhoneNumber}"] = p => p.User.PhoneNumber,
                ["{CreatedAt}"] = p => $"{p.CreatedAt:dd-MM-yyyy}",
                ["{IsActive}"] = p => p.IsActive ? "مفعل" : "غير مفعل",
                ["{BusinessName}"] = p => p.BusinessName
            }
        };

        var result = await _exportService.ExportData(request);

        if (!result.Succeeded)
        {
            return BadRequest(result.ValidationErrors);
        }

        switch (query.ExportFormat)
        {
            case EnumExportFormat.Excel:
                return File((byte[])result.Data,
                                          "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                          $"Admins-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.xlsx");
            case EnumExportFormat.CSV:

                var encWithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
                byte[] bytes;

                var contentType = "text/csv";
                var fileName = $"Admins-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.csv";

                var encodedFileName = Uri.EscapeDataString(fileName);

                using (var memoryStream = new MemoryStream())
                using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
                {
                    writer.Write(result.Data);
                    writer.Flush();

                    bytes = Encoding.UTF8.GetPreamble().Concat(memoryStream.ToArray()).ToArray();
                }

                Response.Headers.Append("Content-Disposition", $"attachment; filename*=UTF-8''{encodedFileName}");
                return new FileContentResult(bytes, contentType)
                {

                    FileDownloadName = encodedFileName
                };
            case EnumExportFormat.PDF:
                return new FileContentResult(Convert.FromBase64String(result.Data), "application/pdf")
                {
                    FileDownloadName = $"Admins-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.pdf"
                };
        }
        return BadRequest();
    }
}

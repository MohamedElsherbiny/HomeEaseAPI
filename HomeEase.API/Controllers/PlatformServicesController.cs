using HomeEase.Application.Commands.PlatformService;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Queries.PlatformService;
using HomeEase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace HomeEase.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class PlatformServicesController(IMediator _mediator, IWebHostEnvironment _webHostEnvironment, IDataExportService _exportService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreatePlatformServiceCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(new { id });
    }

    [HttpGet]
    [AllowAnonymous]
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

    [HttpGet("Export")]
    public async Task<IActionResult> Export([FromQuery] GetAllPlatformServicesQuery query)
    {
        var services = await _mediator.Send(query);

        var request = new ExportRequest<BasePlatformServiceDto>
        {
            Data = services.Items,
            ExportFormat = query.ExportFormat,
            TemplatePath = Path.Combine(_webHostEnvironment.WebRootPath, "ExporTemplates", "PlatformServices"),
            ColumnMappings = new Dictionary<string, Func<BasePlatformServiceDto, string>>
            {
                ["{Name}"] = p => p.Name,
                ["{CreatedAt}"] = p => $"{p.CreatedAt:dd-MM-yyyy}",
                ["{IsActive}"] = p => p.IsActive ? "مفعل" : "غير مفعل"
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
                                          $"PlatformServices-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.xlsx");
            case EnumExportFormat.CSV:

                var encWithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
                byte[] bytes;

                var contentType = "text/csv";
                var fileName = $"PlatformServices-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.csv";

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
                    FileDownloadName = $"PlatformServices-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.pdf"
                };
        }
        return BadRequest();
    }
}

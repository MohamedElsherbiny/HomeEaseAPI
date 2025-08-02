using HomeEase.Application.Commands.ProviderCommands;
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
public class ProvidersController(IMediator _mediator, IWebHostEnvironment _webHostEnvironment, IDataExportService _exportService, ICurrentUserService service) : ControllerBase
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
        return Ok(await _mediator.Send(new GetProviderByUserIdQuery { UserId = service.UserId }));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProviderSearchResultDto>>> SearchProviders([FromQuery] SearchProvidersQuery query)
    {
        return Ok(await _mediator.Send(query));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ProviderOnly")]
    public async Task<IActionResult> UpdateProvider(Guid id, UpdateProviderDto updateDto)
    {
        return Ok(await _mediator.Send(new UpdateProviderCommand { ProviderId = id, ProviderDto = updateDto }));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteProvider(Guid id)
    {
        return Ok(await _mediator.Send(new DeleteProviderCommand { ProviderId = id }));
    }

    [HttpPost("verify/{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> VerifyProvider(Guid id)
    {
        return Ok(await _mediator.Send(new VerifyProviderCommand { ProviderId = id }));
    }

    [HttpPut("schedule/{id}")]
    [Authorize(Policy = "ProviderOnly")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateProviderSchedule(Guid id, ProviderScheduleDto scheduleDto)
    {
        return Ok(await _mediator.Send(new UpdateProviderScheduleCommand { ProviderId = id, ScheduleDto = scheduleDto }));
    }

    [HttpPatch("activate/{id}")]
    public async Task<IActionResult> ActivateProvider(Guid id)
    {
        return Ok(await _mediator.Send(new ActivateProviderCommand(id)));
    }


    [HttpPatch("deactivate/{id}")]
    public async Task<IActionResult> DeactivateProvider(Guid id)
    {
        return Ok(await _mediator.Send(new DeactivateProviderCommand(id)));
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

using HomeEase.Application.Commands.AdminCommands;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Queries.AdminQueries;
using HomeEase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/admins")]
[Authorize(Policy = "AdminOnly")]
public class AdminController(IMediator _mediator, IWebHostEnvironment _webHostEnvironment, IDataExportService _exportService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{adminId}")]
    public async Task<IActionResult> DeleteAdmin(Guid adminId)
    {
        var result = await _mediator.Send(new DeleteAdminCommand(adminId));
        return result ? Ok("Admin deleted successfully.") : BadRequest("Failed to delete admin.");
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetAllAdmins(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetAllAdminsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending,
            IsActive = isActive
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("Export")]
    public async Task<IActionResult> Export([FromQuery] GetAllAdminsQuery query)
    {
        var admins = await _mediator.Send(query);

        var request = new ExportRequest<UserDto>
        {
            Data = admins.Items,
            ExportFormat = query.ExportFormat,
            TemplatePath = Path.Combine(_webHostEnvironment.WebRootPath, "ExporTemplates", "Admins"),
            ColumnMappings = new Dictionary<string, Func<UserDto, string>>
            {
                ["{FirstName}"] = p => p.FirstName,
                ["{LastName}"] = p => p.LastName,
                ["{Email}"] = p => p.Email,
                ["{PhoneNumber}"] = p => p.PhoneNumber,
                ["{CreatedAt}"] = p => $"{p.CreatedAt:dd-MM-yyyy}"
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
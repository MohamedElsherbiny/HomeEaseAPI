using HomeEase.Application.Commands.UserCommends;
using HomeEase.Application.DTOs;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Application.Queries.UserQueries;
using HomeEase.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class UsersController(IMediator _mediator, IWebHostEnvironment _webHostEnvironment, IDataExportService _exportService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(userId));
        return Ok(result);
    }

    [HttpPatch("activate/{id}")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var command = new ActivateUserCommand(id);
        var result = await _mediator.Send(command);
        return result ? Ok("User activation successful.") : BadRequest("Failed to activate user.");
    }

    [HttpPatch("deactivate/{id}")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var command = new DeactivateUserCommand(id);
        var result = await _mediator.Send(command);
        return result ? Ok("User deactivation successful.") : BadRequest("Failed to deactivate user.");
    }

    [HttpGet("Export")]
    public async Task<IActionResult> Export([FromQuery] GetAllUsersQuery query)
    {
        var users = await _mediator.Send(query);

        var request = new ExportRequest<UserDto>
        {
            Data = users.Items,
            ExportFormat = query.ExportFormat,
            TemplatePath = Path.Combine(_webHostEnvironment.WebRootPath, "ExporTemplates", "Users"),
            ColumnMappings = new Dictionary<string, Func<UserDto, string>>
            {
                ["{FirstName}"] = p => p.FirstName,
                ["{LastName}"] = p => p.LastName,
                ["{Email}"] = p => p.Email,
                ["{PhoneNumber}"] = p => p.PhoneNumber,
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
                                          $"Users-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.xlsx");
            case EnumExportFormat.CSV:

                var encWithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
                byte[] bytes;

                var contentType = "text/csv";
                var fileName = $"Users-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.csv";

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
                    FileDownloadName = $"Users-{DateTime.Now:dd-MM-yyyy_HH-mm-ss}.pdf"
                };
        }
        return BadRequest();
    }
}

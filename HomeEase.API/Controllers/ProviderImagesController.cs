using Azure.Storage.Blobs;
using HomeEase.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using AutoMapper;
using HomeEase.Application.DTOs;
using HomeEase.Infrastructure.Data;
using MediatR;
using HomeEase.Application.Commands.ProviderImages;
using HomeEase.Application.Queries.ProviderImages;

namespace HomeEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProviderImagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProviderImagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllProviderImagesQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProviderImageByIdQuery(id));
        return result != null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Create([FromForm] CreateProviderImageCommand command)
    {
        var result = await _mediator.Send(command);

        if (command.ImageType == ImageType.Gallery)
            return Created("", result);
        else
            return CreatedAtAction(nameof(GetById), new { id = result.FirstOrDefault()?.Id }, result.FirstOrDefault());
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProviderImageCommand command)
    {
        command.Id = id;
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProviderImageCommand(id));
        return NoContent();
    }
}

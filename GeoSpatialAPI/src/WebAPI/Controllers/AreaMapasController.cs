using Application.Features.AreaMapas.Commands;
using Application.Features.AreaMapas.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AreaMapasController : ControllerBase
{
    private readonly IMediator _mediator;

    public AreaMapasController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<AreaMapaDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAreaMapasQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<AreaMapaDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAreaMapaByIdQuery(id));
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<AreaMapaDto>>> Create([FromBody] CreateAreaMapaRequest request)
    {
        var command = new CreateAreaMapaCommand(
            request.Name,
            request.Description,
            new List<List<List<double>>> { request.Coordinates }); // Wrap in outer list
        
        var result = await _mediator.Send(command);
        if (!result.Success || result.Data == null)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<AreaMapaDto>>> Update(Guid id, [FromBody] UpdateAreaMapaRequest request)
    {
        var command = new UpdateAreaMapaCommand(
            id,
            request.Name,
            request.Description,
            request.Coordinates != null ? new List<List<List<double>>> { request.Coordinates } : null);
        
        var result = await _mediator.Send(command);
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteAreaMapaCommand(id));
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }
}

public record CreateAreaMapaRequest(
    string Name,
    string? Description,
    List<List<double>> Coordinates // [[lng, lat], [lng, lat], ...] closed ring
);

public record UpdateAreaMapaRequest(
    string Name,
    string? Description,
    List<List<double>> Coordinates
);

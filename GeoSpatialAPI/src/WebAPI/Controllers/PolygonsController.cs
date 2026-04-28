using Application.Features.Polygons.Commands;
using Application.Features.Polygons.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PolygonsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PolygonsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<PolygonDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetPolygonsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<PolygonDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPolygonByIdQuery(id));
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Result<PolygonDto>>> Create([FromBody] CreatePolygonRequest request)
    {
        var command = new CreatePolygonCommand(
            request.Name,
            request.Description,
            new List<List<List<double>>> { request.Coordinates }); // Wrap in outer list
        
        var result = await _mediator.Send(command);
        if (!result.Success || result.Data == null)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<PolygonDto>>> Update(Guid id, [FromBody] UpdatePolygonRequest request)
    {
        var command = new UpdatePolygonCommand(
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
        var result = await _mediator.Send(new DeletePolygonCommand(id));
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }
}

public record CreatePolygonRequest(
    string Name,
    string? Description,
    List<List<double>> Coordinates // [[lng, lat], [lng, lat], ...] closed ring
);

public record UpdatePolygonRequest(
    string Name,
    string? Description,
    List<List<double>> Coordinates
);

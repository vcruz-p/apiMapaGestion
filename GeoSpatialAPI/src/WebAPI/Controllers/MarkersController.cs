using Application.Common.Models;
using Application.Features.Markers.Commands;
using Application.Features.Markers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarkersController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarkersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all markers for the current organization
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<MarkerDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetMarkersQuery());
        return Ok(result);
    }

    /// <summary>
    /// Get marker by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<MarkerDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetMarkerByIdQuery(id));
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>
    /// Get markers near a location
    /// </summary>
    [HttpGet("nearby")]
    public async Task<ActionResult<Result<IEnumerable<MarkerDto>>>> GetNearby(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusInMeters = 1000)
    {
        var result = await _mediator.Send(new GetNearbyMarkersQuery(latitude, longitude, radiusInMeters));
        return Ok(result);
    }

    /// <summary>
    /// Create a new marker
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Result<MarkerDto>>> Create([FromBody] CreateMarkerRequest request)
    {
        var command = new CreateMarkerCommand(
            request.Name,
            request.Description,
            request.Latitude,
            request.Longitude,
            request.Metadata);
        
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);
    }

    /// <summary>
    /// Update an existing marker
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<MarkerDto>>> Update(Guid id, [FromBody] UpdateMarkerRequest request)
    {
        var command = new UpdateMarkerCommand(
            id,
            request.Name,
            request.Description,
            request.Latitude,
            request.Longitude,
            request.Metadata);
        
        var result = await _mediator.Send(command);
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }

    /// <summary>
    /// Soft delete a marker
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteMarkerCommand(id));
        if (!result.Success)
            return NotFound(result);
        
        return Ok(result);
    }
}

public record CreateMarkerRequest(
    string Name,
    string? Description,
    double Latitude,
    double Longitude,
    Dictionary<string, object>? Metadata = null
);

public record UpdateMarkerRequest(
    string Name,
    string? Description,
    double Latitude,
    double Longitude,
    Dictionary<string, object>? Metadata = null
);

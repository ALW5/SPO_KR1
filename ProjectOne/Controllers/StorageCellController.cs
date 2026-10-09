using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectOne.Application.StorageCell.CreateStorageCell;
using ProjectOne.Application.StorageCell.DeleteStorageCell;
using ProjectOne.Application.StorageCell.GetStorageCell;
using ProjectOne.Application.StorageCell.PatchStorageCell;
using ProjectOne.Application.StorageCell.ReserveStorageCell;
using ProjectOne.Application.StorageCell.UpdateStorageCell;
using ProjectOne.Domain.Enum;
using ProjectOne.Extensions;

namespace ProjectOne.Controllers;

[ApiController]
[Route("[controller]")]
public class StorageCellController : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<Guid>> AddStorageCellAsync(
        [FromServices] CreateStorageCellHandler createStorageCellHandler,
        [FromBody] CreateStorageCellRequest createStorageCellRequest,
        CancellationToken cancellationToken = default)
    {
        var storageCell = await createStorageCellHandler.CreateAsync(
            createStorageCellRequest, cancellationToken);

        return storageCell.ToResponse();
    }
    [HttpPost("{id:guid}/reserve")]
    [Authorize(Roles = $"{Roles.Administrator}, {Roles.Client}")]
    public async Task<ActionResult> ReserveAsync(
        [FromServices] ReserveStorageCellHandler handler,
        Guid id,
        [FromBody] ReserveStorageCellRequest request,
        CancellationToken ct = default)
    {
        var userIdClaim = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim is null)
            return Unauthorized();

        if (!Guid.TryParse(userIdClaim.Value, out var clientId))
            return Unauthorized();

        var result = await handler.ReserveAsync(
            id,
            clientId,
            request,
            ct);

        return result.IsSuccess
            ? NoContent()
            : result.Error.ToResponse();
    }
    
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<GetStorageCellResponse>> GetStorageCellAsync(
        [FromServices] GetStorageCellHandler handler,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.GetAsync(id, cancellationToken);
        return result.ToResponse();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<GetStorageCellResponse>>> GetStorageCellAsync(
        [FromServices] GetAllStorageCellHandler handler,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<Guid>> UpdateStorageCellAsync(
        [FromServices] UpdateStorageCellHandler handler,
        Guid id,
        [FromBody] UpdateStorageCellRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.UpdateAsync(id, request, cancellationToken);
        return result.ToResponse();
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<Guid>> PatchStorageCellAsync(
        [FromServices] PatchStorageCellHandler handler,
        Guid id,
        [FromBody] PatchStorageCellRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.PatchAsync(id, request, cancellationToken);
        return result.ToResponse();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles =  Roles.Administrator)]
    public async Task<ActionResult> DeleteStorageCellAsync(
        [FromServices] DeleteStorageCellHandler handler,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? NoContent() : result.Error.ToResponse();
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectOne.Application.Rentalagreement.CreateRentalagreement;
using ProjectOne.Application.Rentalagreement.DeleteRentalagreement;
using ProjectOne.Application.Rentalagreement.GetRentalagreement;
using ProjectOne.Application.Rentalagreement.UpdateRentalagreement;
using ProjectOne.Domain.Enum;
using ProjectOne.Extensions;

namespace ProjectOne.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Administrator}, {Roles.Client}")]
public class RentalagreementController : ControllerBase
{
    private bool IsAdmin => User.IsInRole(Roles.Administrator);

    private bool TryGetUserId(out Guid userId)
        => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);

    [HttpPost]
    public async Task<ActionResult<Guid>> AddRentalAgreementAsync(
        [FromServices] CreateRentalagreementHandler createRentalagreementHandler,
        [FromBody] CreateRentalagreementRequest createRentalagreementRequest,
        CancellationToken cancellationToken = default)
    {
        // Клиент берётся из токена, а не из параметров запроса.
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var rentalagreement = await createRentalagreementHandler.CreateAsync(
            createRentalagreementRequest, userId, cancellationToken);

        return rentalagreement.ToResponse();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetRentalagreementResponse>> GetRentalagreementAsync(
        [FromServices] GetRentalagreementHandler handler,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var result = await handler.GetAsync(id, userId, IsAdmin, cancellationToken);
        return result.ToResponse();
    }

    // Клиент видит только свои договоры, администратор - все.
    [HttpGet]
    public async Task<ActionResult<List<GetRentalagreementResponse>>> GetAllRentalagreementAsync(
        [FromServices] GetAllRentalagreementHandler handler,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var result = await handler.GetAllAsync(userId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    // Изменение сроков и суммы договора - только администратор
    // (иначе клиент мог бы сам поставить себе любую цену).
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    public async Task<ActionResult<Guid>> UpdateRentalagreementAsync(
        [FromServices] UpdateRentalagreementHandler handler,
        Guid id,
        [FromBody] UpdateRentalagreementRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await handler.UpdateAsync(id, request, cancellationToken);
        return result.ToResponse();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteRentalagreementAsync(
        [FromServices] DeleteRentalagreementHandler handler,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var result = await handler.DeleteAsync(id, userId, IsAdmin, cancellationToken);
        return result.IsSuccess ? NoContent() : result.Error.ToResponse();
    }
}

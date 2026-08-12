using System.Security.Claims;
using FGames.Modules.Promotions.Application.Commands;
using FGames.Modules.Promotions.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FGames.Modules.Promotions.Api.Controllers;

[ApiController]
[Route("api/promotions")]
public sealed class PromotionsController : ControllerBase
{
    private readonly ISender _sender;

    public PromotionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListActive(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListActivePromotionsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(CreatePromotionRequest request, CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreatePromotionCommand(request.StartDate, request.EndDate, request.DiscountPercentage, adminId);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{promotionId:int}/games/{gameId:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> AttachGame(int promotionId, Guid gameId, CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _sender.Send(new AttachGamePromotionCommand(promotionId, gameId, adminId), cancellationToken);
        return result.ToActionResult();
    }
}

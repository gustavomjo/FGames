using System.Security.Claims;
using FGames.Modules.Library.Application.Commands;
using FGames.Modules.Library.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FGames.Modules.Library.Api.Controllers;

[ApiController]
[Route("api/library")]
[Authorize(Roles = "User")]
public sealed class LibraryController : ControllerBase
{
    private readonly ISender _sender;

    public LibraryController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> Purchase(PurchaseGameRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _sender.Send(new PurchaseGameCommand(userId, request.GameId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("mine")]
    public async Task<IActionResult> ListMine(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _sender.Send(new ListMyLibraryQuery(userId), cancellationToken);
        return result.ToActionResult();
    }
}

using System.Security.Claims;
using FGames.Modules.Games.Application.Commands;
using FGames.Modules.Games.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FGames.Modules.Games.Api.Controllers;

[ApiController]
[Route("api/games")]
public sealed class GamesController : ControllerBase
{
    private readonly ISender _sender;

    public GamesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ListPublished(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListPublishedGamesQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("draft")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> ListDraft(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListDraftGamesQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetGameByIdQuery(id), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(CreateGameRequest request, CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateGameCommand(request.Name, request.Description, request.Category, request.Rating, request.Price, adminId);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Update(Guid id, UpdateGameRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateGameCommand(id, request.Name, request.Description, request.Category, request.Rating, request.Price);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PublishGameCommand(id), cancellationToken);
        return result.ToActionResult();
    }
}

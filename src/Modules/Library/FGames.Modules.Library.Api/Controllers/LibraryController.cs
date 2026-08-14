using System.Security.Claims;
using FGames.Modules.Library.Application.Commands;
using FGames.Modules.Library.Application.DTOs;
using FGames.Modules.Library.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FGames.Modules.Library.Api.Controllers;

/// <summary>Compra e biblioteca pessoal de jogos.</summary>
/// <remarks>
/// Todas as operações exigem uma conta ativa com função <c>User</c>. Administradores não compram jogos
/// por essas rotas, pois a política é baseada especificamente na função de usuário comum.
/// </remarks>
[ApiController]
[Route("api/library")]
[Authorize(Roles = "User")]
[Produces("application/json")]
public sealed class LibraryController : ControllerBase
{
    private readonly ISender _sender;

    public LibraryController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Adquire um jogo para o usuário autenticado.</summary>
    /// <remarks>
    /// O usuário é identificado pelo token JWT. O jogo precisa existir e estar `Published`. O valor salvo em
    /// `pricePaid` é o preço final no momento da compra, já com promoção ativa quando aplicável. O mesmo usuário
    /// não pode adquirir o mesmo jogo duas vezes.
    /// </remarks>
    /// <param name="request">ID do jogo publicado.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Compra registrada e item incluído na biblioteca.</response>
    /// <response code="400">Dados inválidos ou jogo indisponível.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Conta não possui função User ou está inativa/bloqueada.</response>
    /// <response code="404">Jogo não encontrado.</response>
    /// <response code="409">O jogo já pertence à biblioteca do usuário.</response>
    [HttpPost("purchase")]
    [ProducesResponseType(typeof(UserGameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Purchase(PurchaseGameRequest request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _sender.Send(new PurchaseGameCommand(userId, request.GameId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Lista os jogos adquiridos pelo usuário autenticado.</summary>
    /// <remarks>
    /// O usuário é identificado pelo token. Cada item mostra o ID do jogo, o preço efetivamente pago e a data
    /// da aquisição; o endpoint não retorna jogos de outros usuários.
    /// </remarks>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Itens da biblioteca; pode ser uma lista vazia.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Conta não possui função User ou está inativa/bloqueada.</response>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(IReadOnlyList<UserGameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListMine(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _sender.Send(new ListMyLibraryQuery(userId), cancellationToken);
        return result.ToActionResult();
    }
}

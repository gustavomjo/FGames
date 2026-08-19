using System.Security.Claims;
using FGames.Modules.Promotions.Application.Commands;
using FGames.Modules.Promotions.Application.DTOs;
using FGames.Modules.Promotions.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FGames.Modules.Promotions.Api.Controllers;

/// <summary>Promoções aplicadas aos jogos do catálogo.</summary>
/// <remarks>A consulta de promoções ativas é pública. Criação e associação de jogos exigem administrador.</remarks>
[ApiController]
[Route("api/promotions")]
[Produces("application/json")]
public sealed class PromotionsController : ControllerBase
{
    private readonly ISender _sender;

    public PromotionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Lista as promoções ativas no momento da consulta.</summary>
    /// <remarks>
    /// Endpoint público. Uma promoção é retornada quando está marcada como ativa e o instante atual está
    /// dentro do intervalo entre `startDate` e `endDate`. Cada item inclui os IDs dos jogos associados.
    /// </remarks>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Promoções ativas; pode ser uma lista vazia.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<PromotionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListActive(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListActivePromotionsQuery(), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Cria uma promoção.</summary>
    /// <remarks>
    /// Exige administrador. As datas devem formar um intervalo válido e o desconto deve respeitar os
    /// limites do domínio. A criação não associa jogos automaticamente; use a rota de associação depois.
    /// Datas e horas devem ser enviadas em ISO 8601, preferencialmente em UTC, por exemplo
    /// `2026-08-14T18:00:00Z`.
    /// </remarks>
    /// <param name="request">Período e percentual de desconto.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Promoção criada.</response>
    /// <response code="400">Período ou percentual inválido.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Usuário autenticado não é administrador.</response>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(CreatePromotionRequest request, CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreatePromotionCommand(request.StartDate, request.EndDate, request.DiscountPercentage, adminId);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Associa um jogo a uma promoção.</summary>
    /// <remarks>
    /// Exige administrador. Os dois IDs são informados na rota. O mesmo vínculo não pode ser criado duas vezes,
    /// e as regras da aplicação impedem associações promocionais incompatíveis ou sobrepostas para o jogo.
    /// Após a associação, uma promoção ativa passa a influenciar `finalPrice` nas consultas de jogos e na compra.
    /// </remarks>
    /// <param name="promotionId">Identificador inteiro da promoção.</param>
    /// <param name="gameId">Identificador GUID do jogo.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Promoção atualizada com o jogo associado.</response>
    /// <response code="400">Associação viola uma regra de negócio.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Usuário autenticado não é administrador.</response>
    /// <response code="404">Promoção ou jogo não encontrado.</response>
    /// <response code="409">O mesmo jogo já está vinculado à mesma promoção.</response>
    [HttpPost("{promotionId:int}/games/{gameId:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AttachGame(int promotionId, Guid gameId, CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _sender.Send(new AttachGamePromotionCommand(promotionId, gameId, adminId), cancellationToken);
        return result.ToActionResult();
    }
}

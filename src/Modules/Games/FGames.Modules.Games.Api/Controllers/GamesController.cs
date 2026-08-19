using System.Security.Claims;
using FGames.Modules.Games.Application.Commands;
using FGames.Modules.Games.Application.DTOs;
using FGames.Modules.Games.Application.Queries;
using FGames.Modules.Games.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FGames.Modules.Games.Api.Controllers;

/// <summary>
/// Catálogo de jogos da plataforma.
/// </summary>
/// <remarks>
/// A leitura do catálogo é pública, mas jogos ainda não publicados são visíveis somente para administradores.
/// Criação, edição e publicação também exigem a função <c>Administrator</c>.
/// </remarks>
[ApiController]
[Route("api/games")]
[Produces("application/json")]
public sealed class GamesController : ControllerBase
{
    private readonly ISender _sender;

    public GamesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista jogos e permite filtrar por situação.
    /// </summary>
    /// <remarks>
    /// **Acesso público:** sem autenticação, ou com um usuário comum, a resposta contém somente jogos
    /// `Published`, mesmo quando o filtro é omitido. O filtro `Published` também pode ser informado.
    ///
    /// **Administrador:** sem o filtro, retorna `Draft`, `Published` e `Inactive`; com o filtro,
    /// retorna apenas a situação escolhida. Um usuário não administrador recebe 403 ao tentar consultar
    /// `Draft` ou `Inactive`.
    ///
    /// Valores do filtro: `Draft` (0), `Published` (1) e `Inactive` (2).
    /// O preço final já considera uma promoção ativa, quando existir.
    /// </remarks>
    /// <param name="status">Situação opcional do jogo. Para valores não publicados, é obrigatório ser administrador.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Lista de jogos permitidos para o solicitante.</response>
    /// <response code="400">Valor de filtro inválido.</response>
    /// <response code="403">Usuário sem permissão tentou consultar jogos não publicados.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<GameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List([FromQuery] GameStatus? status, CancellationToken cancellationToken)
    {
        var isAdministrator = User.IsInRole("Administrator");

        if (!isAdministrator && status.HasValue && status.Value != GameStatus.Published)
            return Forbid();

        var effectiveStatus = isAdministrator ? status : GameStatus.Published;
        var result = await _sender.Send(new ListGamesQuery(effectiveStatus), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtém um jogo pelo identificador.
    /// </summary>
    /// <remarks>
    /// A consulta é pública para jogos `Published`. Jogos `Draft` ou `Inactive` retornam 404 para visitantes
    /// e usuários comuns, evitando expor itens fora do catálogo. Administradores podem consultar qualquer situação.
    /// </remarks>
    /// <param name="id">Identificador GUID do jogo.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Jogo encontrado e visível para o solicitante.</response>
    /// <response code="404">Jogo inexistente ou não publicado para o solicitante atual.</response>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var includeUnpublished = User.IsInRole("Administrator");
        var result = await _sender.Send(new GetGameByIdQuery(id, includeUnpublished), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Cria um jogo em rascunho.
    /// </summary>
    /// <remarks>
    /// Exige administrador. Todo jogo nasce com situação `Draft`; use `POST /api/games/{id}/publish`
    /// para publicá-lo. O nome é aparado e deve ser único sem diferenciar maiúsculas, minúsculas ou
    /// espaços nas extremidades. Por exemplo, `Halo`, ` halo ` e `HALO` são considerados o mesmo nome.
    /// Guarde o campo `id` da resposta para editar ou publicar o jogo depois.
    /// </remarks>
    /// <param name="request">Dados do novo jogo.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Jogo criado em rascunho.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Usuário autenticado não é administrador.</response>
    /// <response code="409">Já existe outro jogo com o mesmo nome normalizado.</response>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateGameRequest request, CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateGameCommand(request.Name, request.Description, request.Category, request.Rating, request.Price, adminId);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Atualiza os dados de um jogo.
    /// </summary>
    /// <remarks>
    /// Exige administrador. A situação do jogo não é alterada. O novo nome continua sujeito à regra de
    /// unicidade normalizada; o próprio jogo é desconsiderado nessa verificação.
    /// </remarks>
    /// <param name="id">Identificador GUID do jogo.</param>
    /// <param name="request">Novos dados do jogo.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Jogo atualizado.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Usuário autenticado não é administrador.</response>
    /// <response code="404">Jogo não encontrado.</response>
    /// <response code="409">O novo nome já pertence a outro jogo.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, UpdateGameRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateGameCommand(id, request.Name, request.Description, request.Category, request.Rating, request.Price);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Publica um jogo.
    /// </summary>
    /// <remarks>
    /// Exige administrador. Altera a situação de `Draft` para `Published`, fazendo o jogo aparecer nas
    /// consultas públicas e permitindo sua aquisição. Tentar publicar um jogo já publicado retorna 400.
    /// </remarks>
    /// <param name="id">Identificador GUID retornado na criação ou na listagem administrativa.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Jogo publicado.</response>
    /// <response code="400">Jogo já estava publicado ou não pode ser publicado.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Usuário autenticado não é administrador.</response>
    /// <response code="404">Jogo não encontrado.</response>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PublishGameCommand(id), cancellationToken);
        return result.ToActionResult();
    }
}

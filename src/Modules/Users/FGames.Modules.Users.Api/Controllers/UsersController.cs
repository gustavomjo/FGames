using System.Security.Claims;
using FGames.Modules.Users.Application.Commands;
using FGames.Modules.Users.Application.DTOs;
using FGames.Modules.Users.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FGames.Modules.Users.Api.Controllers;

/// <summary>
/// Cadastro, autenticação e administração de usuários.
/// </summary>
/// <remarks>
/// Registro e login são públicos. O perfil próprio exige autenticação; listagem, consulta por ID,
/// criação administrativa e mudança de situação exigem a função <c>Administrator</c>.
/// </remarks>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Registra um novo usuário comum.</summary>
    /// <remarks>
    /// Endpoint público. O usuário é criado com função `User` e situação `Active`. O e-mail é normalizado
    /// para minúsculas e deve ser único; `Pessoa@Email.com` e `pessoa@email.com` representam a mesma conta.
    /// A senha é armazenada somente como hash e não aparece na resposta.
    /// </remarks>
    /// <param name="request">Dados do cadastro.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Usuário criado.</response>
    /// <response code="400">Dados inválidos, incluindo e-mail, senha ou data de nascimento.</response>
    /// <response code="409">E-mail já cadastrado.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.Name, request.Email, request.Password, request.BirthDate, HttpContext.Connection.RemoteIpAddress?.ToString());
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Autentica um usuário e emite um token JWT.</summary>
    /// <remarks>
    /// Endpoint público. Use `accessToken` no botão **Authorize** do Swagger ou no cabeçalho
    /// `Authorization: Bearer &lt;token&gt;`. Contas inativas ou bloqueadas não podem entrar.
    /// </remarks>
    /// <param name="request">E-mail e senha.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Token, expiração em UTC e dados do usuário autenticado.</response>
    /// <response code="400">Formato dos dados inválido.</response>
    /// <response code="401">Credenciais inválidas ou conta sem permissão para entrar.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Obtém o perfil do usuário autenticado.</summary>
    /// <remarks>O identificador é extraído do token JWT; não é necessário informar um ID na rota.</remarks>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Perfil do usuário autenticado.</response>
    /// <response code="401">Token ausente, inválido ou expirado.</response>
    /// <response code="404">Usuário do token não foi encontrado.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _sender.Send(new GetUserByIdQuery(userId), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Lista todos os usuários.</summary>
    /// <remarks>Exige administrador. Inclui usuários ativos, inativos e bloqueados.</remarks>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Lista de usuários.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Usuário autenticado não é administrador.</response>
    [HttpGet]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListUsersQuery(), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Obtém qualquer usuário pelo identificador.</summary>
    /// <remarks>Exige administrador.</remarks>
    /// <param name="id">Identificador GUID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Usuário encontrado.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Usuário autenticado não é administrador.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Cria um usuário com função escolhida pelo administrador.</summary>
    /// <remarks>
    /// Exige administrador. Diferentemente do registro público, permite escolher `User` (0) ou
    /// `Administrator` (1). A conta nasce ativa e registra o administrador responsável pela criação.
    /// </remarks>
    /// <param name="request">Dados do usuário e função desejada.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Usuário criado.</response>
    /// <response code="400">Dados ou função inválidos.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Usuário autenticado não é administrador.</response>
    /// <response code="409">E-mail já cadastrado.</response>
    [HttpPost]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateUserCommand(request.Name, request.Email, request.Password, request.BirthDate, adminId, request.Role);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Altera a situação de um usuário.</summary>
    /// <remarks>
    /// Exige administrador. Valores: `Active` (0), `Inactive` (1) e `Blocked` (2).
    /// Usuários inativos ou bloqueados deixam de acessar endpoints protegidos e não conseguem efetuar login.
    /// </remarks>
    /// <param name="id">Identificador GUID do usuário.</param>
    /// <param name="request">Nova situação.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <response code="200">Situação atualizada.</response>
    /// <response code="400">Situação inválida.</response>
    /// <response code="401">Token ausente ou inválido.</response>
    /// <response code="403">Usuário autenticado não é administrador.</response>
    /// <response code="404">Usuário não encontrado.</response>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(Guid id, SetUserStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetUserStatusCommand(id, request.Status), cancellationToken);
        return result.ToActionResult();
    }
}

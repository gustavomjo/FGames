using FGames.Modules.Users.Application.Commands;
using FGames.Modules.Users.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace FGames.Modules.Users.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.Name, request.Email, request.Password, request.BirthDate, HttpContext.Connection.RemoteIpAddress?.ToString());
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _sender.Send(new GetUserByIdQuery(userId), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListUsersQuery(), cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var command = new CreateUserCommand(request.Name, request.Email, request.Password, request.BirthDate, adminId, request.Role);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> SetStatus(Guid id, SetUserStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SetUserStatusCommand(id, request.Status), cancellationToken);
        return result.ToActionResult();
    }
}

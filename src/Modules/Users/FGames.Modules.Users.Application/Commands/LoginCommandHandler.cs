using FGames.Modules.Users.Application.DTOs;
using FGames.Modules.Users.Application.Interfaces;
using FGames.Modules.Users.Domain.Enums;
using FGames.Modules.Users.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Users.Application.Commands;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    private static readonly Error InvalidCredentials = new("User.InvalidCredentials", "E-mail ou senha inválidos.");

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<Result<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return Result.Failure<AuthResultDto>(InvalidCredentials);

        if (user.Status != UserStatus.Active)
            return Result.Failure<AuthResultDto>(new Error("User.NotActive", "Este usuário não está ativo."));

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResultDto>(InvalidCredentials);

        var token = _tokenGenerator.GenerateToken(user.Id, user.Email.Value, user.Role);

        return Result.Success(new AuthResultDto(token.AccessToken, token.ExpiresAtUtc, UserDto.FromEntity(user)));
    }
}

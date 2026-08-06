using FGames.Modules.Users.Application.DTOs;
using FGames.Modules.Users.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Users.Application.Queries;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        return user is null
            ? Result.Failure<UserDto>(new Error("User.NotFound", "Usuário não encontrado."))
            : Result.Success(UserDto.FromEntity(user));
    }
}

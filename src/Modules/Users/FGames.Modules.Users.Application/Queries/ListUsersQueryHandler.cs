using FGames.Modules.Users.Application.DTOs;
using FGames.Modules.Users.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Users.Application.Queries;

public sealed class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    private readonly IUserRepository _userRepository;

    public ListUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<IReadOnlyList<UserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.ListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<UserDto>>(users.Select(UserDto.FromEntity).ToList());
    }
}

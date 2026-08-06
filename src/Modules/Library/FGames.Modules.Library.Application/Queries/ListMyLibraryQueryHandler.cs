using FGames.Modules.Library.Application.DTOs;
using FGames.Modules.Library.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Library.Application.Queries;

public sealed class ListMyLibraryQueryHandler : IRequestHandler<ListMyLibraryQuery, Result<IReadOnlyList<UserGameDto>>>
{
    private readonly IUserGameRepository _userGameRepository;

    public ListMyLibraryQueryHandler(IUserGameRepository userGameRepository)
    {
        _userGameRepository = userGameRepository;
    }

    public async Task<Result<IReadOnlyList<UserGameDto>>> Handle(ListMyLibraryQuery request, CancellationToken cancellationToken)
    {
        var userGames = await _userGameRepository.ListByUserAsync(request.UserId, cancellationToken);
        return Result.Success<IReadOnlyList<UserGameDto>>(userGames.Select(UserGameDto.FromEntity).ToList());
    }
}

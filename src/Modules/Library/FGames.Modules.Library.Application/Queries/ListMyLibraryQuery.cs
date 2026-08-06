using FGames.Modules.Library.Application.DTOs;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Library.Application.Queries;

public sealed record ListMyLibraryQuery(Guid UserId) : IRequest<Result<IReadOnlyList<UserGameDto>>>;

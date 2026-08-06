using FGames.Modules.Users.Application.DTOs;
using FGames.Modules.Users.Domain.Enums;
using FGames.Modules.Users.Application;
using FGames.Modules.Users.Domain.Interfaces;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Users.Application.Commands;

public sealed class SetUserStatusCommandHandler : IRequestHandler<SetUserStatusCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetUserStatusCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(SetUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<UserDto>(new Error("User.NotFound", "Usuário não encontrado."));

        switch (request.Status)
        {
            case UserStatus.Active:
                user.Activate();
                break;
            case UserStatus.Inactive:
                user.Deactivate();
                break;
            case UserStatus.Blocked:
                user.Block();
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(UserDto.FromEntity(user));
    }
}

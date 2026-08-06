using FGames.Modules.Users.Application.DTOs;
using FGames.Modules.Users.Application.Interfaces;
using FGames.Modules.Users.Domain.Entities;
using FGames.Modules.Users.Domain.Interfaces;
using FGames.Modules.Users.Application;
using FGames.Modules.Users.Domain.ValueObjects;
using FGames.SharedKernel;
using MediatR;

namespace FGames.Modules.Users.Application.Commands;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<UserDto>(emailResult.Errors);

        var passwordResult = Password.Create(request.Password);
        if (passwordResult.IsFailure)
            return Result.Failure<UserDto>(passwordResult.Errors);

        if (await _userRepository.ExistsByEmailAsync(emailResult.Value.Value, cancellationToken))
            return Result.Failure<UserDto>(new Error("User.EmailAlreadyRegistered", "Já existe um usuário cadastrado com este e-mail."));

        var passwordHash = _passwordHasher.Hash(passwordResult.Value.Value);

        var userResult = User.RegisterByAdmin(
            request.Name,
            emailResult.Value,
            passwordHash,
            request.BirthDate,
            request.CreatedByUserId,
            request.Role);

        if (userResult.IsFailure)
            return Result.Failure<UserDto>(userResult.Errors);

        _userRepository.Add(userResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(UserDto.FromEntity(userResult.Value));
    }
}

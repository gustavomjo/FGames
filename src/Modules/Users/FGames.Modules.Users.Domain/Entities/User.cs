using FGames.Modules.Users.Domain.Enums;
using FGames.Modules.Users.Domain.ValueObjects;
using FGames.SharedKernel;

namespace FGames.Modules.Users.Domain.Entities;

public sealed class User : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateOnly? BirthDate { get; private set; }
    public Role Role { get; private set; }
    public UserStatus Status { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreationIp { get; private set; }

    private User(Guid id)
        : base(id)
    {
    }

    public static Result<User> Register(
        string name,
        Email email,
        string passwordHash,
        DateOnly? birthDate,
        string? creationIp)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>(new Error("User.NameRequired", "O nome do usuário é obrigatório."));

        var user = new User(Guid.NewGuid())
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            BirthDate = birthDate,
            Role = Role.User,
            Status = UserStatus.Active,
            CreatedByUserId = null,
            CreatedAt = DateTime.UtcNow,
            CreationIp = creationIp
        };

        return Result.Success(user);
    }

    public static Result<User> RegisterByAdmin(
        string name,
        Email email,
        string passwordHash,
        DateOnly? birthDate,
        Guid createdByUserId,
        Role role)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<User>(new Error("User.NameRequired", "O nome do usuário é obrigatório."));

        var user = new User(Guid.NewGuid())
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            BirthDate = birthDate,
            Role = role,
            Status = UserStatus.Active,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            CreationIp = null
        };

        return Result.Success(user);
    }

    public void Block() => Status = UserStatus.Blocked;

    public void Activate() => Status = UserStatus.Active;

    public void Deactivate() => Status = UserStatus.Inactive;

    public void ChangePasswordHash(string newPasswordHash) => PasswordHash = newPasswordHash;
}

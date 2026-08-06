using FGames.Modules.Users.Domain.Entities;

namespace FGames.Modules.Users.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string Name,
    string Email,
    DateOnly? BirthDate,
    string Role,
    string Status,
    Guid? CreatedByUserId,
    DateTime CreatedAt)
{
    public static UserDto FromEntity(User user) => new(
        user.Id,
        user.Name,
        user.Email.Value,
        user.BirthDate,
        user.Role.ToString(),
        user.Status.ToString(),
        user.CreatedByUserId,
        user.CreatedAt);
}

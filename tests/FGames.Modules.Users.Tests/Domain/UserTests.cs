using FGames.Modules.Users.Domain.Entities;
using FGames.Modules.Users.Domain.Enums;
using FGames.Modules.Users.Domain.ValueObjects;

namespace FGames.Modules.Users.Tests.Domain;

public class UserTests
{
    private static Email ValidEmail => Email.Create("user@example.com").Value;
    private const string ValidPasswordHash = "hashed-password";

    [Fact]
    public void Register_WithValidData_ReturnsUserWithDefaultRoleAndActiveStatus()
    {
        var result = User.Register("Jane Doe", ValidEmail, ValidPasswordHash, birthDate: null, creationIp: "127.0.0.1");

        Assert.True(result.IsSuccess);
        Assert.Equal(Role.User, result.Value.Role);
        Assert.Equal(UserStatus.Active, result.Value.Status);
        Assert.Null(result.Value.CreatedByUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_WithInvalidName_ReturnsFailure(string name)
    {
        var result = User.Register(name, ValidEmail, ValidPasswordHash, birthDate: null, creationIp: "127.0.0.1");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Register_WithNullName_ReturnsFailure()
    {
        var result = User.Register(null!, ValidEmail, ValidPasswordHash, birthDate: null, creationIp: "127.0.0.1");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void RegisterByAdmin_SetsCreatedByUserIdAndRequestedRole()
    {
        var adminId = Guid.NewGuid();

        var result = User.RegisterByAdmin(
            "New Admin",
            ValidEmail,
            ValidPasswordHash,
            birthDate: null,
            createdByUserId: adminId,
            role: Role.Administrator);

        Assert.True(result.IsSuccess);
        Assert.Equal(adminId, result.Value.CreatedByUserId);
        Assert.Equal(Role.Administrator, result.Value.Role);
    }

    [Fact]
    public void Block_SetsStatusToBlocked()
    {
        var user = User.Register("Jane Doe", ValidEmail, ValidPasswordHash, birthDate: null, creationIp: "127.0.0.1").Value;

        user.Block();

        Assert.Equal(UserStatus.Blocked, user.Status);
    }
}

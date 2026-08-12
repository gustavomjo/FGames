using FGames.Modules.Users.Domain.ValueObjects;

namespace FGames.Modules.Users.Tests.Domain;

public class PasswordTests
{
    [Theory]
    [InlineData("Passw0rd!")]
    [InlineData("C0mplex#Pass")]
    [InlineData("Aa1$aaaa")]
    public void Create_WithStrongPassword_ReturnsSuccess(string value)
    {
        var result = Password.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value.Value);
    }

    [Theory]
    [InlineData("Ab1!")]           // too short
    [InlineData("Password!")]      // no digit
    [InlineData("Password1")]      // no special character
    [InlineData("12345678!")]      // no letter
    [InlineData("")]
    [InlineData("        ")]
    public void Create_WithWeakPassword_ReturnsFailure(string value)
    {
        var result = Password.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithoutUppercaseButAllOtherRules_ReturnsSuccess()
    {
        var result = Password.Create("password1!");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_WithNull_ReturnsFailure()
    {
        var result = Password.Create(null!);

        Assert.True(result.IsFailure);
    }
}

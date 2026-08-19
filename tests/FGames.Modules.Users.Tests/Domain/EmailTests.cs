using FGames.Modules.Users.Domain.ValueObjects;

namespace FGames.Modules.Users.Tests.Domain;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("first.last@sub.example.com")]
    [InlineData("user+tag@example.co")]
    public void Create_WithValidFormat_ReturnsSuccess(string value)
    {
        var result = Email.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    [InlineData("user@example")]
    [InlineData("user example.com")]
    public void Create_WithInvalidFormat_ReturnsFailure(string value)
    {
        var result = Email.Create(value);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithNull_ReturnsFailure()
    {
        var result = Email.Create(null!);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_NormalizesWhitespaceAndCasing()
    {
        var result = Email.Create("  User.Name@Example.COM  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("user.name@example.com", result.Value.Value);
    }

    [Fact]
    public void Emails_DifferingOnlyByCasing_AreEqual()
    {
        var first = Email.Create("user@example.com").Value;
        var second = Email.Create("USER@EXAMPLE.COM").Value;

        Assert.Equal(first, second);
    }
}

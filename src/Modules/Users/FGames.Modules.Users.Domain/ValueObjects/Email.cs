using System.Text.RegularExpressions;
using FGames.SharedKernel;

namespace FGames.Modules.Users.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>(new Error("Email.Empty", "O e-mail não pode ser vazio."));

        var normalizedValue = value.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalizedValue))
            return Result.Failure<Email>(new Error("Email.InvalidFormat", "O e-mail informado não é válido."));

        return Result.Success(new Email(normalizedValue));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}

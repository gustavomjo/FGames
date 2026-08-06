using System.Text.RegularExpressions;
using FGames.SharedKernel;

namespace FGames.Modules.Users.Domain.ValueObjects;

public sealed partial class Password : ValueObject
{
    private const int MinLength = 8;

    public string Value { get; }

    private Password(string value)
    {
        Value = value;
    }

    public static Result<Password> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Password>(new Error("Password.Empty", "A senha não pode ser vazia."));

        var errors = new List<Error>();

        if (value.Length < MinLength)
            errors.Add(new Error("Password.TooShort", $"A senha deve ter no mínimo {MinLength} caracteres."));

        if (!LetterRegex().IsMatch(value))
            errors.Add(new Error("Password.MissingLetter", "A senha deve conter ao menos uma letra."));

        if (!DigitRegex().IsMatch(value))
            errors.Add(new Error("Password.MissingDigit", "A senha deve conter ao menos um número."));

        if (!SpecialCharacterRegex().IsMatch(value))
            errors.Add(new Error("Password.MissingSpecialCharacter", "A senha deve conter ao menos um caractere especial."));

        return errors.Count > 0
            ? Result.Failure<Password>(errors)
            : Result.Success(new Password(value));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    [GeneratedRegex(@"[a-zA-Z]")]
    private static partial Regex LetterRegex();

    [GeneratedRegex(@"\d")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9]")]
    private static partial Regex SpecialCharacterRegex();
}

namespace FGames.Api.RateLimiting;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public RateLimitPolicyOptions Auth { get; set; } = new() { PermitLimit = 5, WindowSeconds = 60 };
    public RateLimitPolicyOptions Global { get; set; } = new() { PermitLimit = 100, WindowSeconds = 60 };
}

public sealed class RateLimitPolicyOptions
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}

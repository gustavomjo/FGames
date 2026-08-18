namespace FGames.Api.Logging;

public sealed class RequestResponseLoggingOptions
{
    public const string SectionName = "Logging:RequestResponseLogging";

    public bool LogRequestBody { get; set; }
    public bool LogResponseBody { get; set; }
    public int MaxBodyLength { get; set; } = 4096;
}

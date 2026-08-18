using System.Text;
using System.Text.RegularExpressions;
using FGames.Api.Logging;
using Microsoft.Extensions.Options;

namespace FGames.Api.Middleware;

public sealed class RequestResponseLoggingMiddleware
{
    public const string RequestBodyItemKey = "RequestBody";
    public const string ResponseBodyItemKey = "ResponseBody";

    private static readonly Regex SensitiveFieldPattern = new(
        "(\"(?:password|senha|token|accessToken|secret|key|token)\"\\s*:\\s*)\"[^\"]*\"",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly RequestDelegate _next;
    private readonly RequestResponseLoggingOptions _options;

    public RequestResponseLoggingMiddleware(RequestDelegate next, IOptions<RequestResponseLoggingOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.LogRequestBody && !_options.LogResponseBody)
        {
            await _next(context);
            return;
        }

        if (_options.LogRequestBody && HasBody(context.Request))
        {
            context.Request.EnableBuffering();
            var requestBody = await ReadAndTruncateAsync(context.Request.Body, _options.MaxBodyLength);
            context.Request.Body.Position = 0;
            context.Items[RequestBodyItemKey] = Redact(requestBody);
        }

        if (!_options.LogResponseBody)
        {
            await _next(context);
            return;
        }

        var originalResponseBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);

            buffer.Position = 0;
            var responseBody = await ReadAndTruncateAsync(buffer, _options.MaxBodyLength);
            context.Items[ResponseBodyItemKey] = Redact(responseBody);

            buffer.Position = 0;
            await buffer.CopyToAsync(originalResponseBody);
        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }

    private static bool HasBody(HttpRequest request) =>
        request.ContentLength is > 0 && !IsMultipart(request.ContentType);

    private static bool IsMultipart(string? contentType) =>
        contentType is not null && contentType.Contains("multipart/", StringComparison.OrdinalIgnoreCase);

    private static async Task<string> ReadAndTruncateAsync(Stream stream, int maxLength)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var content = await reader.ReadToEndAsync();
        return content.Length > maxLength ? content[..maxLength] + "...(truncado)" : content;
    }

    private static string Redact(string body) =>
        SensitiveFieldPattern.Replace(body, "$1\"***\"");
}

using System.Net;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FGames.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException validationException)
        {
            _logger.LogWarning(validationException, "Validation failed for {Path}", context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.BadRequest, "Erro de validação", validationException.Errors
                .Select(failure => new { failure.PropertyName, failure.ErrorMessage }));
        }
        catch (DbUpdateException dbUpdateException)
            when (dbUpdateException.InnerException is PostgresException
                  { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            var postgresException = (PostgresException)dbUpdateException.InnerException!;

            _logger.LogWarning(
                dbUpdateException,
                "Unique constraint {ConstraintName} violated for {Path}",
                postgresException.ConstraintName,
                context.Request.Path);

            var message = postgresException.ConstraintName switch
            {
                "uq_user_email" => "Já existe um usuário cadastrado com este e-mail.",
                "uq_game_name_normalized" => "Já existe um jogo cadastrado com este nome.",
                "uq_user_game" => "Este jogo já foi adquirido por este usuário.",
                "uq_game_promotion" => "Este jogo já está vinculado a esta promoção.",
                _ => "Já existe um registro com os mesmos dados."
            };

            await WriteProblemAsync(
                context,
                HttpStatusCode.Conflict,
                "Conflito de dados",
                new[] { new { ErrorMessage = message } });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);
            await WriteProblemAsync(context, HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado.", null);
        }
    }

    private static Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string title, object? errors)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsJsonAsync(new
        {
            title,
            status = (int)statusCode,
            errors
        });
    }
}

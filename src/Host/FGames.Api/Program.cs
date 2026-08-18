using FGames.Api.Adapters;
using FGames.Api.Logging;
using FGames.Api.Middleware;
using FGames.Api.RateLimiting;
using FGames.Modules.Games.Infrastructure;
using FGames.Modules.Games.Infrastructure.Persistence;
using FGames.Modules.Library.Infrastructure;
using FGames.Modules.Library.Infrastructure.Persistence;
using FGames.Modules.Promotions.Infrastructure;
using FGames.Modules.Promotions.Infrastructure.Persistence;
using FGames.Modules.Users.Infrastructure;
using FGames.Modules.Users.Infrastructure.Auth;
using FGames.Modules.Users.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' não configurada.");

builder.Services.Configure<RequestResponseLoggingOptions>(
    builder.Configuration.GetSection(RequestResponseLoggingOptions.SectionName));

// Rate Limiting: policy "auth" mais restritiva para login/registro (alvo de brute-force / criação em massa
// de contas) e um limiter global mais permissivo para os demais endpoints. Particionado por IP, em memória
// (sem Redis) — adequado para instância única; para múltiplas instâncias, precisaria de um store distribuído.
var rateLimitingOptions = builder.Configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
    ?? new RateLimitingOptions();

builder.Services.AddRateLimiter(limiterOptions =>
{
    limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = rateLimitingOptions.Global.PermitLimit,
            Window = TimeSpan.FromSeconds(rateLimitingOptions.Global.WindowSeconds),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    limiterOptions.AddPolicy("auth", httpContext =>
    {
        var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = rateLimitingOptions.Auth.PermitLimit,
            Window = TimeSpan.FromSeconds(rateLimitingOptions.Auth.WindowSeconds),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    limiterOptions.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                title = "Muitas requisições. Tente novamente mais tarde.",
                status = StatusCodes.Status429TooManyRequests,
                errors = (object?)null
            },
            options: null,
            contentType: "application/problem+json",
            cancellationToken);
    };
});

// Módulos: Application (MediatR + FluentValidation) e Infrastructure (EF Core, Auth)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(FGames.Modules.Users.Application.Commands.RegisterUserCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(FGames.Modules.Games.Application.Commands.CreateGameCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(FGames.Modules.Promotions.Application.Commands.CreatePromotionCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(FGames.Modules.Library.Application.Commands.PurchaseGameCommand).Assembly);

    cfg.AddOpenBehavior(typeof(FGames.Modules.Users.Application.Common.ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(FGames.Modules.Games.Application.Common.ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(FGames.Modules.Promotions.Application.Common.ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(FGames.Modules.Library.Application.Common.ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(typeof(FGames.Modules.Users.Application.Commands.RegisterUserCommand).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(FGames.Modules.Games.Application.Commands.CreateGameCommand).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(FGames.Modules.Promotions.Application.Commands.CreatePromotionCommand).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(FGames.Modules.Library.Application.Commands.PurchaseGameCommand).Assembly);

builder.Services.AddUsersInfrastructure(builder.Configuration, connectionString);
builder.Services.AddGamesInfrastructure(connectionString);
builder.Services.AddPromotionsInfrastructure(connectionString);
builder.Services.AddLibraryInfrastructure(connectionString);

// Adapters cross-módulo (só o Host enxerga mais de um módulo ao mesmo tempo)
builder.Services.AddScoped<FGames.Modules.Promotions.Application.Interfaces.IGameLookupService, PromotionsGameLookupServiceAdapter>();
builder.Services.AddScoped<FGames.Modules.Library.Application.Interfaces.IGameLookupService, LibraryGameLookupServiceAdapter>();
builder.Services.AddScoped<FGames.Modules.Library.Application.Interfaces.IActivePromotionLookupService, LibraryActivePromotionLookupServiceAdapter>();
builder.Services.AddScoped<FGames.Modules.Games.Application.Interfaces.IActivePromotionLookupService, GamesActivePromotionLookupServiceAdapter>();

// Autenticação JWT
var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
var jwtSecret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret não configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// API
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FIAP Cloud Games API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token JWT (sem o prefixo 'Bearer')."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<UsersDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<GamesDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<PromotionsDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<LibraryDbContext>().Database.Migrate();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        if (httpContext.Items.TryGetValue(RequestResponseLoggingMiddleware.RequestBodyItemKey, out var requestBody))
        {
            diagnosticContext.Set(RequestResponseLoggingMiddleware.RequestBodyItemKey, requestBody);
        }

        if (httpContext.Items.TryGetValue(RequestResponseLoggingMiddleware.ResponseBodyItemKey, out var responseBody))
        {
            diagnosticContext.Set(RequestResponseLoggingMiddleware.ResponseBodyItemKey, responseBody);
        }
    };
});
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Host encerrado inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}

using FGames.Api;
using FGames.Api.Adapters;
using FGames.Api.Behaviors;
using FGames.Api.Middleware;
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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' não configurada.");

// Módulos: Application (MediatR + FluentValidation) e Infrastructure (EF Core, Auth)
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(FGames.Modules.Users.Application.Commands.RegisterUserCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(FGames.Modules.Games.Application.Commands.CreateGameCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(FGames.Modules.Promotions.Application.Commands.CreatePromotionCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(FGames.Modules.Library.Application.Commands.PurchaseGameCommand).Assembly);

    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FIAP Cloud Games API",
        Version = "v1",
        Description = "API para cadastro de usuários, catálogo de jogos, promoções e biblioteca. " +
                      "Cada operação informa seu nível de acesso, regras de negócio e respostas possíveis."
    });

    var apiAssemblies = new[]
    {
        typeof(FGames.Modules.Users.Api.Controllers.UsersController).Assembly,
        typeof(FGames.Modules.Games.Api.Controllers.GamesController).Assembly,
        typeof(FGames.Modules.Promotions.Api.Controllers.PromotionsController).Assembly,
        typeof(FGames.Modules.Library.Api.Controllers.LibraryController).Assembly
    };

    foreach (var assembly in apiAssemblies.Distinct())
    {
        var xmlFile = Path.Combine(AppContext.BaseDirectory, $"{assembly.GetName().Name}.xml");
        if (File.Exists(xmlFile))
            options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);
    }

    options.OperationFilter<SwaggerAuthorizeCheckOperationFilter>();

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<ActiveUserMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

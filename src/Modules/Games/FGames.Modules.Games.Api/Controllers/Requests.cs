using FGames.Modules.Games.Domain.Enums;

namespace FGames.Modules.Games.Api.Controllers;

/// <summary>Dados necessários para cadastrar um jogo.</summary>
/// <param name="Name">Nome único, com no máximo 150 caracteres. A comparação ignora caixa e espaços nas extremidades.</param>
/// <param name="Description">Descrição opcional do jogo.</param>
/// <param name="Category">Categoria: Action=0, Adventure=1, RPG=2, Strategy=3, Sports=4, Simulation=5, Educational=6, Other=7.</param>
/// <param name="Rating">Classificação etária: Everyone=0, Ten=1, Twelve=2, Fourteen=3, Sixteen=4, Eighteen=5.</param>
/// <param name="Price">Preço não negativo.</param>
public sealed record CreateGameRequest(string Name, string? Description, GameCategory Category, AgeRating Rating, decimal Price);

/// <summary>Dados substituídos durante a atualização de um jogo.</summary>
/// <param name="Name">Nome único, com no máximo 150 caracteres. A comparação ignora caixa e espaços nas extremidades.</param>
/// <param name="Description">Descrição opcional do jogo.</param>
/// <param name="Category">Categoria: Action=0, Adventure=1, RPG=2, Strategy=3, Sports=4, Simulation=5, Educational=6, Other=7.</param>
/// <param name="Rating">Classificação etária: Everyone=0, Ten=1, Twelve=2, Fourteen=3, Sixteen=4, Eighteen=5.</param>
/// <param name="Price">Preço não negativo.</param>
public sealed record UpdateGameRequest(string Name, string? Description, GameCategory Category, AgeRating Rating, decimal Price);

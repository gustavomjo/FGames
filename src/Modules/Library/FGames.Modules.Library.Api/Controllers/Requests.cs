namespace FGames.Modules.Library.Api.Controllers;

/// <summary>Dados necessários para adquirir um jogo.</summary>
/// <param name="GameId">Identificador GUID de um jogo publicado.</param>
public sealed record PurchaseGameRequest(Guid GameId);

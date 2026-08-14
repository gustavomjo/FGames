namespace FGames.Modules.Promotions.Api.Controllers;

/// <summary>Dados para criação de uma promoção.</summary>
/// <param name="StartDate">Início do período em ISO 8601, preferencialmente UTC.</param>
/// <param name="EndDate">Fim do período em ISO 8601, posterior ao início.</param>
/// <param name="DiscountPercentage">Percentual de desconto aplicado ao preço do jogo.</param>
public sealed record CreatePromotionRequest(DateTime StartDate, DateTime EndDate, decimal DiscountPercentage);

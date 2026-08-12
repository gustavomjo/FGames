namespace FGames.Modules.Promotions.Api.Controllers;

public sealed record CreatePromotionRequest(DateTime StartDate, DateTime EndDate, decimal DiscountPercentage);

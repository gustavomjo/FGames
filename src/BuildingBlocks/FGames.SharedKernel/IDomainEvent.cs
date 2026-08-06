namespace FGames.SharedKernel;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}

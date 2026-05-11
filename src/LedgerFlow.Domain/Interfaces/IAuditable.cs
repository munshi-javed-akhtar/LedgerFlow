namespace LedgerFlow.Domain.Interfaces;

public interface IAuditable
{
    DateTimeOffset CreatedAtUtc { get; }
}

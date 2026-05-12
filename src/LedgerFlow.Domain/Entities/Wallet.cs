using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Domain.Entities;

public class Wallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal Balance { get; set; }
    public WalletStatus Status { get; set; } = WalletStatus.Active;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}

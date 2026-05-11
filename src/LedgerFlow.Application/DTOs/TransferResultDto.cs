namespace LedgerFlow.Application.DTOs;

public sealed record TransferResultDto(Guid TransactionId, string Status, DateTimeOffset ProcessedAtUtc);

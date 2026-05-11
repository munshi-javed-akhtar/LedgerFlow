namespace LedgerFlow.Application.DTOs;

public sealed record TransferRequestDto(
    Guid SourceWalletId,
    Guid DestinationWalletId,
    decimal Amount,
    string Currency,
    string IdempotencyKey,
    Guid UserId);

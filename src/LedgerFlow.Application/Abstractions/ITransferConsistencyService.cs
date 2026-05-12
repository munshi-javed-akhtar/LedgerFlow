using LedgerFlow.Application.DTOs;

namespace LedgerFlow.Application.Abstractions;

public interface ITransferConsistencyService
{
    Task<TransferResultDto> TransferAsync(TransferRequestDto request, CancellationToken cancellationToken);
}

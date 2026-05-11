using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.DTOs;
using MediatR;

namespace LedgerFlow.Application.Transactions.Commands.Transfer;

public sealed class TransferCommandHandler(ITransferConsistencyService transferConsistencyService)
    : IRequestHandler<TransferCommand, TransferResultDto>
{
    public Task<TransferResultDto> Handle(TransferCommand request, CancellationToken cancellationToken)
        => transferConsistencyService.TransferAsync(request.Request, cancellationToken);
}

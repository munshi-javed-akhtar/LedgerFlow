using LedgerFlow.Application.DTOs;
using MediatR;

namespace LedgerFlow.Application.Transactions.Commands.Transfer;

public sealed record TransferCommand(TransferRequestDto Request) : IRequest<TransferResultDto>;

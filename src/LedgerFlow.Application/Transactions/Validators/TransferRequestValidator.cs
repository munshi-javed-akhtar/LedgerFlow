using FluentValidation;
using LedgerFlow.Application.DTOs;

namespace LedgerFlow.Application.Transactions.Validators;

public sealed class TransferRequestValidator : AbstractValidator<TransferRequestDto>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.SourceWalletId).NotEmpty();
        RuleFor(x => x.DestinationWalletId).NotEmpty().NotEqual(x => x.SourceWalletId);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.IdempotencyKey).NotEmpty().MaximumLength(100);
    }
}

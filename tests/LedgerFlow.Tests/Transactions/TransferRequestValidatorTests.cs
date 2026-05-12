using LedgerFlow.Application.DTOs;
using LedgerFlow.Application.Transactions.Validators;

namespace LedgerFlow.Tests.Transactions;

public class TransferRequestValidatorTests
{
    [Fact]
    public void Invalid_Request_Should_Fail()
    {
        var validator = new TransferRequestValidator();
        var result = validator.Validate(new TransferRequestDto(Guid.Empty, Guid.Empty, -1, "US", "", Guid.Empty));
        Assert.False(result.IsValid);
    }
}

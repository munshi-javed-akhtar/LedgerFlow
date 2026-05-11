using Dapper;
using LedgerFlow.Application.Abstractions;
using LedgerFlow.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace LedgerFlow.Persistence.Services;

public sealed class TransferConsistencyService(string connectionString) : ITransferConsistencyService
{
    public async Task<TransferResultDto> TransferAsync(TransferRequestDto request, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var txId = Guid.NewGuid();

        var updated = await connection.ExecuteAsync(new CommandDefinition(@"
            UPDATE wallets SET balance = balance - @amount
            WHERE id = @sourceWalletId AND balance >= @amount AND status = 1;

            UPDATE wallets SET balance = balance + @amount
            WHERE id = @destinationWalletId AND status = 1;

            INSERT INTO transactions (id, sourcewalletid, destinationwalletid, amount, currency, type, status, createdatutc)
            VALUES (@txId, @sourceWalletId, @destinationWalletId, @amount, @currency, 'Transfer', 'Completed', NOW());",
            new
            {
                request.SourceWalletId,
                request.DestinationWalletId,
                request.Amount,
                request.Currency,
                txId
            }, transaction: transaction, cancellationToken: cancellationToken));

        if (updated < 3)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new DbUpdateException("Transfer failed due to consistency checks.");
        }

        await transaction.CommitAsync(cancellationToken);
        return new TransferResultDto(txId, "Completed", DateTimeOffset.UtcNow);
    }
}

using LedgerFlow.Application.Abstractions;
using LedgerFlow.Persistence.Context;
using LedgerFlow.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerFlow.Persistence.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? "Host=postgres;Port=5432;Database=ledgerflow;Username=postgres;Password=postgres";

        services.AddDbContext<LedgerFlowDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ITransferConsistencyService>(_ => new TransferConsistencyService(connectionString));
        return services;
    }
}

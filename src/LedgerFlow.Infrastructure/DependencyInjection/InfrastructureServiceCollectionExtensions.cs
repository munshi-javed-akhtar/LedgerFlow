using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace LedgerFlow.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ResiliencePipeline>(new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions { MaxRetryAttempts = 3, Delay = TimeSpan.FromSeconds(2) })
            .Build());
        return services;
    }
}

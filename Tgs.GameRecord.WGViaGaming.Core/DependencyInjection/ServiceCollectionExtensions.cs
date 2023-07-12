using System.Reflection;
using Tgs.GameCommand.Client;
using Tgs.GameInterface.WGViaGaming.Client;
using Tgs.GameRecord.WGViaGaming.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecordCore(
       this IServiceCollection services,
       Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
       Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
       Action<GameInterfaceWGViaGamingClientOptions, IServiceProvider> gameInterfaceWGViaGamingClientOptions)
        => services
        .AddOptions<GameRecordInfoOptions>()
        .Configure(gameRecordInfoOptions)
        .Services
        .RegisterAllServiceUtilityTypes(
            Assembly.GetExecutingAssembly(),
            ServiceLifetime.Transient)
        .AddGameRoundsWorkClient()
        .AddGameCommandClient(gameCommandClientOptions)
        .AddGameInterfaceWGViaGamingClient(gameInterfaceWGViaGamingClientOptions)
        .AddTransient<IUtensil>(sp => ActivatorUtilities.CreateInstance<UtensilRepository>(sp, TimeSpan.FromHours(1)))
        .AddTransient<IHangfireUtensil, HangfireUtensil>();
}

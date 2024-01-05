using Tgs.GameCommand.Client;
using Tgs.GameInterface.WGViaGaming.Client;
using Tgs.GameRecord.Pinnacle;
using Tgs.GameRecord.WGViaGaming.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameRecord(
       this IServiceCollection services,
       Action<RedisOptions, IServiceProvider> redisDBconfigure,
       Action<GameRecordInfoOptions, IServiceProvider> gameRecordInfoOptions,
       Action<GameCommandClientOptions, IServiceProvider> gameCommandClientOptions,
       Action<GameInterfaceWGViaGamingClientOptions, IServiceProvider> gameInterfaceWGViaGamingClientOptions)
        => services
        .AddOptions<RedisOptions>()
        .Configure(redisDBconfigure)
        .Services
        .AddOptions<GameRecordInfoOptions>()
        .Configure(gameRecordInfoOptions)
        .Services
        .AddSingleton<RedisConnectionManager>()
        .AddGameRecordCore(
            gameCommandClientOptions: gameCommandClientOptions,
            gameInterfaceWGViaGamingClientOptions: gameInterfaceWGViaGamingClientOptions,
            redisDatabaseResolver: (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);
}

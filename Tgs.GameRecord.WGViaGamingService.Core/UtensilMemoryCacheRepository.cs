using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Tgs.GameProviders.QueryServer.Models;

namespace Tgs.GameRecord.WGViaGamingService.Core
{
    internal class UtensilMemoryCacheRepository : IUtensil
    {
        private readonly TimeSpan _absoluteExpirationRelativeToNow;
        private readonly GameRecordInfoOptions _gameRecordInfo;
        private readonly IUtensil _lEGaming;
        private readonly IMemoryCache _memoryCache;

        public UtensilMemoryCacheRepository(
            TimeSpan absoluteExpirationRelativeToNow,
            IOptions<GameRecordInfoOptions> options,
            IUtensil LEGaming,
            IMemoryCache memoryCache)
        {
            _absoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
            _lEGaming = LEGaming ?? throw new ArgumentNullException(nameof(LEGaming));
            _gameRecordInfo = options.Value;
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async ValueTask<Guid?> BrandGetGameProviderConfigurationIdAsync(Guid brandId, Guid gameProviderId)
        {
            var key = $"{_gameRecordInfo.GameProviderCode}:{brandId}:{gameProviderId}";
            if (!_memoryCache.TryGetValue(key, out Guid? gameProviderConfigurationId))
            {
                gameProviderConfigurationId = await _lEGaming.BrandGetGameProviderConfigurationIdAsync(brandId, gameProviderId).ConfigureAwait(false);

                if (gameProviderConfigurationId == null)
                {
                    return null;
                }
                _ = _memoryCache.Set(key, gameProviderConfigurationId, absoluteExpirationRelativeToNow: _absoluteExpirationRelativeToNow);
            }
            return gameProviderConfigurationId;
        }

        public async ValueTask<int?> GetGameIdAsync(Guid gameProviderId, string gameExternalId)
        {
            var key = $"{_gameRecordInfo.GameProviderCode}:{gameExternalId}";
            if (!_memoryCache.TryGetValue(key, out int? gameId))
            {
                gameId = await _lEGaming.GetGameIdAsync(gameProviderId, gameExternalId).ConfigureAwait(false);

                if (gameId.HasValue)
                {
                    _ = _memoryCache.Set(key, gameId, absoluteExpirationRelativeToNow: _absoluteExpirationRelativeToNow);
                }
            }
            return gameId;
        }

        public async ValueTask<Guid> GetGameProviderIdAsync()
        {
            if (!_memoryCache.TryGetValue(_gameRecordInfo.GameProviderCode, out Guid? gameProviderId))
            {
                gameProviderId = await _lEGaming.GetGameProviderIdAsync().ConfigureAwait(false);

                if (gameProviderId.HasValue)
                {
                    _ = _memoryCache.Set(_gameRecordInfo.GameProviderCode, gameProviderId, TimeSpan.FromDays(1));
                }
            }
            return gameProviderId!.Value;
        }

        public async ValueTask<PlayerModel?> GetPlayerAsync(string playerCode)
        {
            var key = $"{_gameRecordInfo.GameProviderCode}:{playerCode}";
            if (!_memoryCache.TryGetValue(key, out PlayerModel? playerModel))
            {
                playerModel = await _lEGaming.GetPlayerAsync(playerCode).ConfigureAwait(false);

                if (playerModel == null)
                {
                    return null;
                }
                _ = _memoryCache.Set(key, playerModel, TimeSpan.FromDays(1));
            }
            return playerModel;
        }
    }
}

using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.GameRecordSchedules.Client;
using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.WGViaGamingService.Core
{
    internal class GameRecordScheduleRepository : IGameRecordSchedule
    {
        private readonly IGameRecordSchedulesClient _client;
        private readonly GameRecordInfoOptions _gameRecordInfo;
        private readonly ISystemClock _systemClock;

        public GameRecordScheduleRepository(
            IOptions<GameRecordInfoOptions> options,
            IGameRecordSchedulesClient client,
            ISystemClock systemClock)
        {
            _gameRecordInfo = options.Value;
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
        }

        public async ValueTask<GameRecordScheduleReply?> GetAsync(CancellationToken cancellationToken = default)
        {
            var request = new GameRecordScheduleGetRequest()
            {
                GameProviderCode = _gameRecordInfo.GameProviderCode,
                BrandCode = string.Empty,
                GameProviderAgentId = string.Empty,
                CurrencyCode = string.Empty,
            };
            return await _client.GameRecordScheduleGetAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public ScheduleConfig GetConfig(GameRecordScheduleReply gameProviderSchedule)
            => JsonSerializer.Deserialize<ScheduleConfig>(gameProviderSchedule.Config)!;

        public async ValueTask<GameRecordScheduleReply?> InitAsync(Guid gameProviderId, CancellationToken cancellationToken = default)
        {
            var sysTime = _systemClock.CurrentTime;

            var config = new ScheduleConfig()
            {
                LastGameRecordTime = new DateTimeOffset(Convert.ToDateTime("1970-01-01")),
                StartTime = DateTimeOffset.Parse(sysTime.ToString("yyyy-MM-dd")).AddDays(_gameRecordInfo.IntervalDay * -1),
                EndTime = DateTimeOffset.Parse(sysTime.ToString("yyyy-MM-dd")),
            };

            var addRequest = new GameRecordScheduleAddRequest()
            {
                GameProviderId = gameProviderId,
                GameProviderCode = _gameRecordInfo.GameProviderCode,
                BrandCode = string.Empty,
                GameProviderAgentId = string.Empty,
                CurrencyCode = string.Empty,
                Config = JsonSerializer.Serialize(config)
            };

            _ = await _client.GameRecordScheduleAddAsync(addRequest, cancellationToken).ConfigureAwait(false);

            return await GetAsync(cancellationToken).ConfigureAwait(false);
        }

        public async ValueTask<GameRecordScheduleReply?> UpdateAsync(GameRecordScheduleReply gameProviderSchedule, DateTimeOffset? lastGameRecordTime, DateTimeOffset sysTime, CancellationToken cancellationToken = default)
        {
            var config = GetConfig(gameProviderSchedule);

            if (config.EndTime > sysTime || (sysTime - config.EndTime).TotalDays < _gameRecordInfo.IntervalDay)
            {
                config.StartTime = sysTime.AddDays(_gameRecordInfo.IntervalDay * -1);
                config.EndTime = sysTime;
            }
            else
            {
                var endTime = config.EndTime.AddDays(1);
                config.StartTime = config.EndTime;
                config.EndTime = (endTime > sysTime) ? sysTime : endTime;
            }

            config.LastGameRecordTime = lastGameRecordTime ?? config.LastGameRecordTime;

            var request = new GameRecordScheduleUpdateRequest()
            {
                GameRecordScheduleId = gameProviderSchedule.GameRecordScheduleId,
                Config = JsonSerializer.Serialize(config)
            };

            var updateResult = await _client.GameRecordScheduleUpdateAsync(request, cancellationToken).ConfigureAwait(false);
            return updateResult.Success
                ? await GetAsync(cancellationToken).ConfigureAwait(false)
                : null;
        }
    }
}

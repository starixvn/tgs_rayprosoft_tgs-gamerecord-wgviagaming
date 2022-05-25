using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameProviders.Client;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices
{
    internal class GameRecordService : IGetService<GameRecordQuery, GameRecordResult>
    {
        private readonly IGameRecordListClient _client;
        private readonly GameRecordInfoOptions _gameRecordInfoOptions;
        private readonly ILogger<GameRecordService> _logger;
        private readonly IMessageSender _messageSender;
        private readonly string _subject;
        private readonly IUtensil _utensil;

        public GameRecordService(
            IOptions<GameRecordInfoOptions> options,
            ILogger<GameRecordService> logger,
            IGameRecordListClient client,
            IMessageSender messageSender,
            IUtensil utensil)
        {
            _gameRecordInfoOptions = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
            _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
            _subject = $"tgs.gp.gamerecord.database.round.{_gameRecordInfoOptions.GameProviderCode.ToLower()}";
        }

        public async ValueTask<GameRecordResult> GetAsync(GameRecordQuery query, CancellationToken cancellationToken = default)
        {
            var count = 0;
            var successCount = 0;
            var failureCount = 0;
            var lastGameRecordTime = default(DateTimeOffset?);
            var page = 1;
            var rows = 1000;
            while (true)
            {
                var response = await _client.WGHorseGameRecordListAsync(
                    query.StartTime,
                    query.EndTime,
                    page,
                    rows).ConfigureAwait(false);

                if (response == null || !response.Success)
                {
                    return new()
                    {
                        Success = false
                    };
                }

                if (response.Success && response.Datas == null)
                {
                    break;
                }

                if (response.Datas!.Length == 0)
                {
                    break;
                }

                page++;

                foreach (var data in response.Datas)
                {
                    count++;

                    var success = true;
                    var gameProviderId = await _utensil.GetGameProviderIdAsync().ConfigureAwait(false);

                    var playerModel = await _utensil.GetPlayerAsync(data.PlayerCode).ConfigureAwait(false);
                    var gameId = await _utensil.GetGameIdAsync(gameProviderId, data.ExternalGameId).ConfigureAwait(false);
                    if (playerModel == null || !gameId.HasValue)
                    {
                        success = false;
                    }

                    Guid? gameProviderConfigurationId = default;
                    if (playerModel != null)
                    {
                        gameProviderConfigurationId = await _utensil.BrandGetGameProviderConfigurationIdAsync(playerModel.BrandId, gameProviderId).ConfigureAwait(false);
                        if (!gameProviderConfigurationId.HasValue)
                        {
                            success = false;
                        }
                    }

                    if (success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                    }

                    var messageRequest = new GameRoundMessageRequest()
                    {
                        GameProviderCode = data.GameProviderCode,
                        GameProviderId = data.GameProviderId,
                        ExternalRoundId = data.ExternalRoundId,
                        PlayerCode = (playerModel == null) ? string.Empty : playerModel.PlayerCode,
                        GameId = gameId ?? -1,
                        ExternalGameId = data.ExternalGameId,
                        CurrencyCode = (playerModel == null) ? string.Empty : playerModel.DefaultWalletCurrencyCode,
                        Status = data.Status,
                        GameProviderConfigurationId = gameProviderConfigurationId ?? Guid.Empty,
                        WalletId = (playerModel == null) ? Guid.Empty : playerModel.DefaultWalletId,
                        BrandId = (playerModel == null) ? Guid.Empty : playerModel.BrandId,
                        ExternalCreatedOn = data.ExternalCreatedOn,
                        ExternalClosedOn = data.ExternalClosedOn,
                        BetAmount = data.BetAmount,
                        ValidBetAmount = data.ValidBetAmount,
                        PlayerWinLoss = data.PlayerWinLoss,
                        Commission = data.Commission,
                        Turnover = data.Turnover,
                        SettleAmount = data.SettleAmount,
                        ClientStatus = data.ClientStatus,
                        AdditionalData = data.AdditionalData,
                        Success = success
                    };

                    _logger.LogInformation("LogOn:{logOn}  Subject:{subject} MessageRequest:{request}", DateTime.Now.ToString("o"), _subject, messageRequest);

                    await _messageSender.PublishAsync(
                        _subject,
                        messageRequest.ToBson(),
                        cancellationToken).ConfigureAwait(false);

                    lastGameRecordTime = data.ExternalClosedOn.HasValue ? data.ExternalClosedOn : data.ExternalCreatedOn;
                }
            }
            return new GameRecordResult()
            {
                Success = true,
                Count = count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                LastGameRecordTime = lastGameRecordTime
            };
        }
    }
}

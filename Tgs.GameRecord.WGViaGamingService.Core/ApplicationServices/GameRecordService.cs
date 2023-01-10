using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Tgs.GameProviders.Client;
using Tgs.GameRounds.Common;
using Tgs.GameRounds.Common.MessageModels;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices;

internal class GameRecordService : IGetService<GameRecordQuery, GameRecordResult>
{
    private readonly IGameRecordListClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly ILogger<GameRecordService> _logger;
    private readonly IMessageSender _messageSender;
    private readonly MongoSettingOptions _mongoSetting;
    private readonly IUtensil _utensil;

    public GameRecordService(
        ILogger<GameRecordService> logger,
        IOptions<GameRecordInfoOptions> gameRecordInfoOptions,
        IOptions<MongoSettingOptions> mongoSettingOptions,
        IGameRecordListClient client,
        IMessageSender messageSender,
        IUtensil utensil)
    {
        _gameRecordInfo = gameRecordInfoOptions.Value;
        _mongoSetting = mongoSettingOptions.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
    }

    public async ValueTask<GameRecordResult> GetAsync(GameRecordQuery query, CancellationToken cancellationToken = default)
    {
        var count = 0;
        var successCount = 0;
        var failureCount = 0;
        var lastGameRecordTime = default(DateTimeOffset?);
        var page = 1;
        var rows = 1000;
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        while (true)
        {
            var response = await _client.WGHorseGameRecordListAsync(
                startTime: query.StartTime,
                endTime: query.EndTime,
                page: page,
                rows: rows,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (response == null || !response.Success)
            {
                return new()
                {
                    Success = false
                };
            }

            var datas = response.Datas ?? Enumerable.Empty<GameRecordModle>();

            if (!datas.Any())
            {
                break;
            }

            page++;

            foreach (var data in datas)
            {
                count++;

                var success = true;

                var playerModel = await _utensil
                    .GetPlayerAsync(
                        playerCode: data.PlayerCode,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var gameId = await _utensil
                    .GetGameIdAsync(
                        gameProviderId: gameProviderId,
                        gameExternalId: data.ExternalGameId,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                if (playerModel == null || !gameId.HasValue)
                {
                    success = false;
                }

                Guid? gameProviderConfigurationId = default;
                if (playerModel != null)
                {
                    gameProviderConfigurationId = await _utensil
                        .GetBrandGameProviderConfigurationIdAsync(
                            brandId: playerModel.BrandId,
                            gameProviderId: gameProviderId,
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
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

                var message = new GameRecordMessage(
                    ExternalRoundId: data.ExternalRoundId,
                    GameProviderId: gameProviderId,
                    GameProviderCode: data.GameProviderCode,
                    GameId: gameId ?? -1,
                    ExternalGameId: data.ExternalGameId,
                    PlayerCode: (playerModel == null) ? string.Empty : playerModel.PlayerCode,
                    CurrencyCode: (playerModel == null) ? string.Empty : playerModel.DefaultWalletCurrencyCode,
                    WalletId: (playerModel == null) ? Guid.Empty : playerModel.DefaultWalletId,
                    BrandId: (playerModel == null) ? Guid.Empty : playerModel.BrandId,
                    GameProviderConfigurationId: gameProviderConfigurationId ?? Guid.Empty,
                    Status: (GameRoundStatus)data.Status,
                    ClientStatus: (GameRoundClientStatus)data.ClientStatus,
                    ExternalCreatedOn: data.ExternalCreatedOn!.Value,
                    ExternalClosedOn: data.ExternalClosedOn,
                    BetAmount: data.BetAmount.GetValueOrDefault(0),
                    ValidBetAmount: data.ValidBetAmount.GetValueOrDefault(0),
                    SettleAmount: data.SettleAmount.GetValueOrDefault(0),
                    Commission: data.Commission.GetValueOrDefault(0),
                    Turnover: data.Turnover.GetValueOrDefault(0),
                    PlayerWinLoss: data.PlayerWinLoss.GetValueOrDefault(0),
                    AdditionalData: data.AdditionalData,
                    Success: success);

                _logger.LogInformation("LogOn:{logOn}  Subject:{subject} MessageRequest:{message}",
                    DateTime.Now.ToString("o"),
                    _mongoSetting.GlobPattern,
                    message);

                await _messageSender
                    .PublishAsync(
                        subject: _mongoSetting.GlobPattern,
                        data: message.ToBson(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

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

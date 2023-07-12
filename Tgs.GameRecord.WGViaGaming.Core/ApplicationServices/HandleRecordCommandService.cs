using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly IGameRoundsWorkClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly IUtensil _utensil;

    public HandleRecordCommandService(
        IOptions<GameRecordInfoOptions> options,
        IUtensil utensil,
        IGameRoundsWorkClient client)
    {
        _gameRecordInfo = options.Value;
        _utensil = utensil ?? throw new ArgumentNullException(nameof(utensil));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var success = true;
        var gameProviderId = await _utensil
            .GetGameProviderIdAsync(
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var gameProviderCode = _utensil.GetGameProviderCode();
        var player = await _utensil
            .GetPlayerAsync(
                playerCode: command.Record.PlayerCode,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        var gameId = await _utensil
            .GetGameIdAsync(
                gameProviderId: gameProviderId,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (player == null)
        {
            success = false;
        }

        Guid? gameProviderConfigurationId = default;
        if (player != null)
        {
            gameProviderConfigurationId = await _utensil
                .GetBrandGameProviderConfigurationIdAsync(
                    brandId: player.BrandId,
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
            var status = RoundStatus.Open;
            var clientStatus = RoundClientStatus.Open;

            var externalCreatedOn = command.Record.ExternalCreatedOn.ToDateTimeOffset();
            var externalClosedOn = (DateTimeOffset?)null;
            var bet = command.Record.Bet.ToDecimal().GetValueOrDefault(0);
            var settle = 0.0m;
            var commission = 0.0m;
            var turnover = 0.0m;
            var playerWinLoss = 0.0m;
            if (command.Record.IsClosed)
            {
                status = RoundStatus.Closed;
                externalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset();
                settle = command.Record.Settle.ToDecimal().GetValueOrDefault(0);
                turnover = command.Record.Bet.ToDecimal().GetValueOrDefault(0);
                playerWinLoss = command.Record.WinLoss.ToDecimal().GetValueOrDefault(0);
                clientStatus = command.Record.IsDraw ? RoundClientStatus.Draw : playerWinLoss > 0 ? RoundClientStatus.Win : RoundClientStatus.Loss;
            }

            await _client
            .RoundBidding(
                gameProviderId: gameProviderId,
                gameProviderCode: gameProviderCode,
                externalRoundId: command.Record.ExternalRoundId,
                gameId: gameId,
                externalGameId: _gameRecordInfo.GameExternalId,
                playerCode: command.Record.PlayerCode,
                walletId: player!.DefaultWalletId,
                currencyCode: player!.DefaultWalletCurrencyCode,
                brandId: player!.BrandId,
                gameProviderConfigurationId: gameProviderConfigurationId!.Value,
                status: status,
                clientStatus: clientStatus,
                externalCreatedOn: externalCreatedOn,
                externalClosedOn: externalClosedOn,
                bet: bet,
                validBet: bet,
                settle: settle,
                commission: commission,
                turnover: turnover,
                playerWinLoss: playerWinLoss,
                additionalData: command.Record.AdditionalData,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
        else
        {
            await _client
            .RoundMissing(
                gameProviderId: gameProviderId,
                gameProviderCode: gameProviderCode,
                externalRoundId: command.Record.ExternalRoundId,
                gameId: gameId,
                externalGameId: _gameRecordInfo.GameExternalId,
                playerCode: command.Record.PlayerCode,
                data: JsonSerializer.Serialize(command.Record),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        }
    }
}

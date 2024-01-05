using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;

internal class HandleRecordCommandService(
    IOptions<GameRecordInfoOptions> options,
    IGameRecordHandleClient client) : ICommandService<HandleRecordCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo = options.Value;

    public ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var wagerState = WagerStates.Running;

        var externalCreatedOn = command.Record.ExternalCreatedOn.ToDateTimeOffset();
        var externalClosedOn = DateTimeOffset.UnixEpoch;
        var bet = command.Record.Bet.ToDecimal().GetValueOrDefault(0);
        var settle = 0.0m;
        var commission = 0.0m;
        var turnover = 0.0m;
        var playerWinLoss = 0.0m;
        if (command.Record.IsClosed)
        {
            externalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset();
            settle = command.Record.Settle.ToDecimal().GetValueOrDefault(0);
            turnover = command.Record.Bet.ToDecimal().GetValueOrDefault(0);
            playerWinLoss = command.Record.WinLoss.ToDecimal().GetValueOrDefault(0);
            wagerState = command.Record.IsDraw ? WagerStates.Draw : playerWinLoss > 0 ? WagerStates.Win : WagerStates.Lose;
        }

        return client
            .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            playerCode: command.Record.PlayerCode,
            wagerState: wagerState,
            wagerType: WagerTypes.Normal,
            freeGameState: FreeGameStates.None,
            gambleState: GambleStates.None,
            cashoutState: CashoutStates.None,
            resettleState: ResettleStates.None,
            externalRoundId: command.Record.ExternalRoundId,
            externalWagerId: command.Record.ExternalRoundId,
            externalGameId: _gameRecordInfo.GameExternalId,
            externalCreatedOn: externalCreatedOn,
            externalClosedOn: externalClosedOn,
            externalPayoffOn: externalClosedOn,
            externalResettleOn: DateTimeOffset.UnixEpoch,
            bet: bet,
            validBet: bet,
            settle: settle,
            commission: commission,
            turnover: turnover,
            winLoss: playerWinLoss,
            additionalData: JsonNode.Parse(command.Record.AdditionalData)!,
            cancellationToken: cancellationToken);
    }
}

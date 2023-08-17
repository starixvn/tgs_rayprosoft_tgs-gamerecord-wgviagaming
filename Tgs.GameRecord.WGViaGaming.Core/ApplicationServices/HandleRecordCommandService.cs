using Microsoft.Extensions.Options;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;

internal class HandleRecordCommandService : ICommandService<HandleRecordCommand>
{
    private readonly IGameRecordHandleClient _client;
    private readonly GameRecordInfoOptions _gameRecordInfo;

    public HandleRecordCommandService(
        IOptions<GameRecordInfoOptions> options,
        IGameRecordHandleClient client)
    {
        _gameRecordInfo = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public ValueTask ExecuteAsync(HandleRecordCommand command, CancellationToken cancellationToken = default)
    {
        var status = GameRecordStatus.Open;
        var clientStatus = GameRecordClientStatus.Open;

        var externalCreatedOn = command.Record.ExternalCreatedOn.ToDateTimeOffset();
        var externalClosedOn = (DateTimeOffset?)null;
        var bet = command.Record.Bet.ToDecimal().GetValueOrDefault(0);
        var settle = 0.0m;
        var commission = 0.0m;
        var turnover = 0.0m;
        var playerWinLoss = 0.0m;
        if (command.Record.IsClosed)
        {
            status = GameRecordStatus.Closed;
            externalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset();
            settle = command.Record.Settle.ToDecimal().GetValueOrDefault(0);
            turnover = command.Record.Bet.ToDecimal().GetValueOrDefault(0);
            playerWinLoss = command.Record.WinLoss.ToDecimal().GetValueOrDefault(0);
            clientStatus = command.Record.IsDraw ? GameRecordClientStatus.Draw : playerWinLoss > 0 ? GameRecordClientStatus.Win : GameRecordClientStatus.Loss;
        }

        return _client
            .MessageAsync(
            gameProviderCode: _gameRecordInfo.GameProviderCode,
            externalRoundId: command.Record.ExternalRoundId,
            externalGameId: _gameRecordInfo.GameExternalId,
            playerCode: command.Record.PlayerCode,
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
            cancellationToken: cancellationToken);
    }
}

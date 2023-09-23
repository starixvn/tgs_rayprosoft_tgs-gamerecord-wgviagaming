using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.WGViaGaming.Protos;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.Handle.Common.Protos;
using Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;

namespace Tgs.GameRecord.WGViaGaming.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
    [Fact]
    public async void Normal_Process()
    {
        var gameRecordInfo = Options.Create(new GameRecordInfoOptions
        {
            GameProviderCode = "WGVIA",
            GameProviderConfiguration = "WGVIAConfiguration",
            GameExternalId = "1",
            GameProviderHangfire = "wgvia-schedule:hangfire:",
            IntervalDays = 2,
            IntervalDaysMax = 30,
            IntervalDaysMinimum = 1
        });
        var fakeGameRoundsWorkClient = Substitute.For<IGameRecordHandleClient>();

        var command = new HandleRecordCommand(
            Record: new GameRecordInfo
            {
                ExternalRoundId = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                PlayerCode = "4ucmqplxb029",
                IsClosed = true,
                IsDraw = false,
                ExternalCreatedOn = DateTimeOffset.Now.AddMonths(-30).ToTimestamp(),
                ExternalClosedOn = DateTimeOffset.Now.ToTimestamp(),
                Bet = 10m,
                Settle = 7.5m,
                WinLoss = 2.5m,
                AdditionalData = "{}"
            });

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeGameRoundsWorkClient);

        await service.ExecuteAsync(command: command);

        await fakeGameRoundsWorkClient
           .Received(1)
           .MessageAsync(
            gameProviderCode: Arg.Is(gameRecordInfo.Value.GameProviderCode),
            playerCode: Arg.Is(command.Record.PlayerCode),
            wagerState: Arg.Is(WagerStates.Win),
            wagerType: Arg.Is(WagerTypes.Normal),
            freeGameState: Arg.Is(FreeGameStates.None),
            gambleState: Arg.Is(GambleStates.None),
            cashoutState: Arg.Is(CashoutStates.None),
            resettleState: Arg.Is(ResettleStates.None),
            externalRoundId: Arg.Is(command.Record.ExternalRoundId),
            externalWagerId: Arg.Is(command.Record.ExternalRoundId),
            externalGameId: Arg.Is(gameRecordInfo.Value.GameExternalId),
            externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn.ToDateTimeOffset()),
            externalClosedOn: Arg.Is(command.Record.ExternalClosedOn.ToDateTimeOffset()),
            externalPayoffOn: Arg.Is(command.Record.ExternalClosedOn.ToDateTimeOffset()),
            externalResettleOn: Arg.Is(DateTimeOffset.UnixEpoch),
            bet: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            validBet: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            settle: Arg.Is(command.Record.Settle.ToDecimal().GetValueOrDefault(0)),
            commission: Arg.Is(0.0m),
            turnover: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
            winLoss: Arg.Is(command.Record.WinLoss.ToDecimal().GetValueOrDefault(0)),
            additionalData: Arg.Is(command.Record.AdditionalData),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}

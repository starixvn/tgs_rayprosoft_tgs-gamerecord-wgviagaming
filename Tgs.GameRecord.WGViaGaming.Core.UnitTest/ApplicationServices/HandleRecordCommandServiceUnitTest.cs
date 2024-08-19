using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameInterface.WGViaGaming.Protos;
using Tgs.GameRecord.Handle.Client;
using Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;

namespace Tgs.GameRecord.WGViaGaming.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Normal_Process()
	{
		var faceLogger = NullLogger<HandleRecordCommandService>.Instance;
		var fakeGameRecordInfoOptions = Options.Create(new GameRecordInfoOptions
		{
			GameProviderCode = "WGVIA",
			GameProviderConfiguration = "WGVIAConfiguration",
			GameExternalId = "1",
			HangfirePrefix = "gamerecord-wgvia-hangfire:",
			HangfireLocking = "gamerecord-wgvia-locking:",
			IntervalDays = 2,
			IntervalDaysMax = 30,
			IntervalDaysMinimum = 1
		});
		var fakePolicySettingsOptions = Options.Create(new PolicySettingsOptions
		{
			RetryCount = 1,
			RetrySeconds = 1,
		});
		var fakeGameRecordHandleClient = Substitute.For<IGameRecordHandleClient>();

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
			faceLogger,
			fakeGameRecordInfoOptions,
			fakePolicySettingsOptions,
			fakeGameRecordHandleClient);

		await service.ExecuteAsync(command: command);

		_ = fakeGameRecordHandleClient
			.Received(1)
			.HandleAsync(
			source: Arg.Is(new Handle.Common.ViewModels.GameRecordHandleViewModel
			{
				GameProviderCode = fakeGameRecordInfoOptions.Value.GameProviderCode,
				PlayerCode = command.Record.PlayerCode,
				ExternalRoundId = command.Record.ExternalRoundId,
				ExternalBetId = command.Record.ExternalRoundId,
				ExternalGameId = fakeGameRecordInfoOptions.Value.GameExternalId,
				ExternalCreatedOn = command.Record.ExternalCreatedOn.ToDateTimeOffset(),
				ExternalClosedOn = command.Record.ExternalClosedOn.ToDateTimeOffset(),
				ExternalPayoffOn = command.Record.ExternalClosedOn.ToDateTimeOffset(),
				ExternalResettleOn = DateTimeOffset.UnixEpoch,
				BetState = Handle.Common.Protos.BetState.Win,
				BetType = Handle.Common.Protos.BetType.Normal,
				FreeGameState = Handle.Common.Protos.FreeGameState.None,
				GambleState = Handle.Common.Protos.GambleState.None,
				CashoutState = Handle.Common.Protos.CashoutState.None,
				ResettleState = Handle.Common.Protos.ResettleState.None,
				JackpotState = Handle.Common.Protos.JackpotState.None,
				Bet = 10m,
				ValidBet = 10m,
				Settle = 7.5m,
				Commission = 0.0M,
				Turnover = 10m,
				WinLoss = 2.5m,
				Jackpot = 0.0M,
				AdditionalData = command.Record.AdditionalData
			}),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}

using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.GameCommand.Common;
using Tgs.GameInterface.WGViaGaming.Protos;
using Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;

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
        var fakeGameRoundsWorkClient = Substitute.For<IGameRoundsWorkClient>();
        var fakeUtensil = Substitute.For<IUtensil>();

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

        var gameProviderId = Guid.NewGuid();
        _ = fakeUtensil
            .GetGameProviderIdAsync(
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderId);

        var gameProviderCode = "WGVIA";
        _ = fakeUtensil
            .GetGameProviderCode()
            .Returns(gameProviderCode);

        var player = new PlayerInfo(
            ResellerId: Guid.NewGuid(),
            ResellerCode: "TGS",
            LicenseeId: Guid.NewGuid(),
            LicenseeCode: "TGS",
            BrandId: Guid.NewGuid(),
            BrandCode: "GPI",
            PlayerCode: "4ucmqplxb029",
            Username: "4ucmqplxb029",
            DefaultWalletId: Guid.NewGuid(),
            DefaultWalletCurrencyCode: "CNY");
        _ = fakeUtensil
            .GetPlayerAsync(
            playerCode: Arg.Is(command.Record.PlayerCode),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(player);

        int gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
            gameProviderId: Arg.Is(gameProviderId),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameId);

        Guid? gameProviderConfigurationId = Guid.NewGuid();
        _ = fakeUtensil
            .GetBrandGameProviderConfigurationIdAsync(
            brandId: Arg.Is(player.BrandId),
            gameProviderId: Arg.Is(gameProviderId),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(gameProviderConfigurationId);

        var service = new HandleRecordCommandService(
            gameRecordInfo,
            fakeUtensil,
            fakeGameRoundsWorkClient);

        await service
            .ExecuteAsync(
            command: command)
            .ConfigureAwait(false);

        await fakeGameRoundsWorkClient
           .Received(1)
           .RoundBidding(
           gameProviderId: Arg.Is(gameProviderId),
           gameProviderCode: Arg.Is(gameProviderCode),
           externalRoundId: Arg.Is(command.Record.ExternalRoundId),
           gameId: Arg.Is(gameId),
           externalGameId: Arg.Is(gameRecordInfo.Value.GameExternalId),
           playerCode: Arg.Is(command.Record.PlayerCode),
           walletId: Arg.Is(player!.DefaultWalletId),
           currencyCode: Arg.Is(player!.DefaultWalletCurrencyCode),
           brandId: Arg.Is(player!.BrandId),
           gameProviderConfigurationId: Arg.Is(gameProviderConfigurationId!.Value),
           status: Arg.Is(RoundStatus.Closed),
           clientStatus: Arg.Is(RoundClientStatus.Win),
           externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn.ToDateTimeOffset()),
           externalClosedOn: Arg.Is(command.Record.ExternalClosedOn.ToDateTimeOffset()),
           bet: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
           validBet: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
           settle: Arg.Is(command.Record.Settle.ToDecimal().GetValueOrDefault(0)),
           commission: Arg.Is(0.0m),
           turnover: Arg.Is(command.Record.Bet.ToDecimal().GetValueOrDefault(0)),
           playerWinLoss: Arg.Is(command.Record.WinLoss.ToDecimal().GetValueOrDefault(0)),
           additionalData: Arg.Is(command.Record.AdditionalData),
           cancellationToken: Arg.Any<CancellationToken>())
           .ConfigureAwait(false);

        await fakeGameRoundsWorkClient
            .Received(0)
            .RoundMissing(
            gameProviderId: Arg.Is(gameProviderId),
            gameProviderCode: Arg.Is(gameProviderCode),
            externalRoundId: Arg.Is(command.Record.ExternalRoundId),
            gameId: Arg.Is(gameId),
            externalGameId: Arg.Is(gameRecordInfo.Value.GameExternalId),
            playerCode: Arg.Is(command.Record.PlayerCode),
            data: Arg.Is(JsonSerializer.Serialize(command.Record)),
            cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}

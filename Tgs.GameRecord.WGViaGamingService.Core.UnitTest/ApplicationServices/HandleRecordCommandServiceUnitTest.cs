using System.Text.Json;
using NSubstitute;
using Tgs.GameCommand.Common;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices;
using Tgs.GameRounds.Work.Client;
using Tgs.GameRounds.Work.Common.Protos;

namespace Tgs.GameRecord.WGViaGamingService.Core.UnitTest.ApplicationServices;

public class HandleRecordCommandServiceUnitTest
{
    [Fact]
    public async void Normal_Process()
    {
        var fakeGameRoundsWorkClient = Substitute.For<IGameRoundsWorkClient>();
        var fakeUtensil = Substitute.For<IUtensil>();

        var command = new HandleRecordCommand(
            Record: new GameRecordModle
            {
                ExternalRoundId = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                ExternalGameId = "1",
                PlayerCode = "4ucmqplxb029",
                Status = GameRecordRoundStatus.Closed,
                ClientStatus = GameRecordRoundClientStatus.Win,
                ExternalCreatedOn = DateTimeOffset.Now.AddMonths(-30),
                ExternalClosedOn = DateTimeOffset.Now,
                BetAmount = 10m,
                ValidBetAmount = 5m,
                SettleAmount = 7.5m,
                Turnover = 5m,
                PlayerWinLoss = 2.5m,
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

        int? gameId = 1;
        _ = fakeUtensil
            .GetGameIdAsync(
            gameProviderId: Arg.Is(gameProviderId),
            gameExternalId: Arg.Is(command.Record.ExternalGameId),
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
           gameId: Arg.Is(gameId ?? -1),
           externalGameId: Arg.Is(command.Record.ExternalGameId),
           playerCode: Arg.Is(command.Record.PlayerCode),
           walletId: Arg.Is(player!.DefaultWalletId),
           currencyCode: Arg.Is(player!.DefaultWalletCurrencyCode),
           brandId: Arg.Is(player!.BrandId),
           gameProviderConfigurationId: Arg.Is(gameProviderConfigurationId!.Value),
           status: Arg.Is(RoundStatus.Closed),
           clientStatus: Arg.Is(RoundClientStatus.Win),
           externalCreatedOn: Arg.Is(command.Record.ExternalCreatedOn!.Value),
           externalClosedOn: Arg.Is(command.Record.ExternalClosedOn),
           bet: Arg.Is(command.Record.BetAmount.GetValueOrDefault(0)),
           validBet: Arg.Is(command.Record.ValidBetAmount.GetValueOrDefault(0)),
           settle: Arg.Is(command.Record.SettleAmount.GetValueOrDefault(0)),
           commission: Arg.Is(0.0m),
           turnover: Arg.Is(command.Record.Turnover.GetValueOrDefault(0)),
           playerWinLoss: Arg.Is(command.Record.PlayerWinLoss.GetValueOrDefault(0)),
           additionalData: Arg.Is(command.Record.AdditionalData),
           cancellationToken: Arg.Any<CancellationToken>())
           .ConfigureAwait(false);

        await fakeGameRoundsWorkClient
            .Received(0)
            .RoundMissing(
            gameProviderId: Arg.Is(gameProviderId),
            gameProviderCode: Arg.Is(gameProviderCode),
            externalRoundId: Arg.Is(command.Record.ExternalRoundId),
            gameId: Arg.Is(gameId ?? -1),
            externalGameId: Arg.Is(command.Record.ExternalGameId),
            playerCode: Arg.Is(command.Record.PlayerCode),
            data: Arg.Is(JsonSerializer.Serialize(command.Record)),
            cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}

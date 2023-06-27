using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameProviders.Client;
using Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGamingService.Core.UnitTest.ApplicationServices;

public class PullRecordCommandServiceUnitTest
{
    [Fact]
    public async void Normal_Process()
    {
        var fakeLogger = NullLogger<PullRecordCommandService>.Instance;
        var fakeClient = Substitute.For<IGameRecordListClient>();
        var fakeService = Substitute.For<ICommandService<HandleRecordCommand>>();

        var command = new PullRecordCommand(
            StartTime: DateTime.UtcNow.AddMinutes(-10),
            EndTime: DateTime.UtcNow);

        var service = new PullRecordCommandService(
            fakeLogger,
            fakeClient,
            fakeService);

        await service
            .ExecuteAsync(
            command: command)
            .ConfigureAwait(false);

        _ = await fakeClient
            .Received(1)
            .WGHorseGameRecordListAsync(
            startTime: Arg.Is(command.StartTime),
            endTime: Arg.Is(command.EndTime),
            page: Arg.Any<int>(),
            rows: Arg.Any<int>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .ConfigureAwait(false);
    }
}
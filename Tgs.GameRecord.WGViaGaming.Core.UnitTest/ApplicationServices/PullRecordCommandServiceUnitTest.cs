using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.GameInterface.WGViaGaming.Client;
using Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGaming.Core.UnitTest.ApplicationServices;

public class PullRecordCommandServiceUnitTest
{
	[Fact]
	public async Task Normal_Process()
	{
		var fakeLogger = NullLogger<PullRecordCommandService>.Instance;
		var fakeClient = Substitute.For<IGameInterfaceWGViaGamingClient>();
		var fakeService = Substitute.For<ICommandService<HandleRecordCommand>>();

		var command = new PullRecordCommand(
			ApiUrl: new Uri("http://localhost:8088/"),
		  PartnerCode: "partnerCode",
		  SecretKey: "secretKey",
		  SecretIV: "secretIV",
		  StartTime: DateTime.UtcNow.AddDays(-1),
		  EndTime: DateTime.UtcNow);

		var service = new PullRecordCommandService(
			fakeLogger,
			fakeClient,
			fakeService);

		await service.ExecuteAsync(command: command);

		_ = fakeClient
			.Received(1)
			.GameRecordAsync(
			apiUrl: Arg.Is(command.ApiUrl),
			partnerCode: Arg.Is(command.PartnerCode),
			secretKey: Arg.Is(command.SecretKey),
			secretIV: Arg.Is(command.SecretIV),
			startTime: Arg.Is(command.StartTime),
			endTime: Arg.Is(command.EndTime),
			pageIndex: Arg.Any<int>(),
			pageSize: Arg.Any<int>(),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}

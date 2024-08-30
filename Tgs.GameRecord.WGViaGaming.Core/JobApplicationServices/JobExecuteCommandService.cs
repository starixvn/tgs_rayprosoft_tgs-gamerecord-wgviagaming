using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameRecord.WGViaGaming.Common;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.WGViaGaming.Core.JobApplicationServices;

internal class JobExecuteCommandService(
	ILogger<JobExecuteCommandService> logger,
	IOptions<GameRecordInfoOptions> options,
	TimeProvider timeProvider,
	IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
	private readonly double _intervalDays = options.Value.IntervalDaysMax < options.Value.IntervalDays
		? options.Value.IntervalDaysMax
		: options.Value.IntervalDaysMinimum > options.Value.IntervalDays
		? options.Value.IntervalDaysMinimum
		: options.Value.IntervalDays;

	public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
	{
		var currentTime = timeProvider.GetUtcNow();
		var endTime = DateTimeOffset.ParseExact(currentTime.ToString(@"yyyy-MM-dd\T00:00:00K"), @"yyyy-MM-dd\T00:00:00K", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);
		var startTime = endTime.AddDays(_intervalDays * -1);

		var message = new WGViaGamingGameRecordMessage
		{
			ApiUrl = command.ApiUrl,
			PartnerCode = command.PartnerCode,
			SecretKey = command.SecretKey,
			SecretIV = command.SecretIV,
			StartTime = startTime,
			EndTime = endTime
		};

		logger.LogInformation("{logInformation}",
			JsonSerializer.Serialize(new
			{
				LogOn = DateTime.Now.ToString("u"),
				Subject = WGViaGamingNatsConst.SchedulePublish,
				Data = new
				{
					command.ApiUrl,
					command.PartnerCode,
					command.SecretKey,
					command.SecretIV,
					StartTime = message.StartTime.ToString("u"),
					EndTime = message.EndTime.ToString("u")
				},
				Message = "This message publishing handler."
			}));

		return messageSender.PublishAsync(
			subject: WGViaGamingNatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(message),
			cancellationToken: cancellationToken);
	}
}

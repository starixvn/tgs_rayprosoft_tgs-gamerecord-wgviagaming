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
    IMessageSender messageSender) : ICommandService<JobExecuteCommand>
{
    private readonly double _intervalDays = options.Value.IntervalDaysMax < options.Value.IntervalDays
        ? options.Value.IntervalDaysMax
        : options.Value.IntervalDaysMinimum > options.Value.IntervalDays
        ? options.Value.IntervalDaysMinimum
        : options.Value.IntervalDays;

    public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
    {
        var endTime = DateTimeOffset.ParseExact(DateTimeOffset.UtcNow.ToString(@"yyyy-MM-dd\T00:00:00K"), @"yyyy-MM-dd\T00:00:00K", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);
        var startTime = endTime.AddDays(_intervalDays * -1);

        var parameters = new WGViaGamingGameRecordMessage
        {
            ApiUrl = command.ApiUrl,
            PartnerCode = command.PartnerCode,
            SecretKey = command.SecretKey,
            SecretIV = command.SecretIV,
            StartTime = startTime,
            EndTime = endTime
        };

        logger.LogDebug("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

        return messageSender.PublishAsync(
            subject: WGViaGamingNatsConst.SchedulePublish,
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters)),
            cancellationToken: cancellationToken);
    }
}

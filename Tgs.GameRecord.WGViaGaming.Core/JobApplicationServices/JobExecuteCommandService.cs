using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.GameRecord.WGViaGaming.Common;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGaming.Core.JobApplicationServices;

internal class JobExecuteCommandService : ICommandService<JobExecuteCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfo;
    private readonly double _intervalDays;
    private readonly ILogger<JobExecuteCommandService> _logger;
    private readonly IMessageSender _messageSender;

    public JobExecuteCommandService(
        ILogger<JobExecuteCommandService> logger,
        IOptions<GameRecordInfoOptions> options,
        IMessageSender messageSender)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _gameRecordInfo = options.Value;
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        _intervalDays = _gameRecordInfo.IntervalDaysMax < _gameRecordInfo.IntervalDays
            ? _gameRecordInfo.IntervalDaysMax
            : _gameRecordInfo.IntervalDaysMinimum > _gameRecordInfo.IntervalDays
            ? _gameRecordInfo.IntervalDaysMinimum
            : _gameRecordInfo.IntervalDays;
    }


    public ValueTask ExecuteAsync(JobExecuteCommand command, CancellationToken cancellationToken = default)
    {
        var endTime = DateTimeOffset.ParseExact(DateTimeOffset.UtcNow.ToString(@"yyyy-MM-dd\T00:00:00K"), @"yyyy-MM-dd\T00:00:00K", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);
        var startTime = endTime.AddDays(_intervalDays * -1);

        var parameters = new GameRecordMessage
        {
            ApiUrl = command.ApiUrl,
            PartnerCode = command.PartnerCode,
            SecretKey = command.SecretKey,
            SecretIV = command.SecretIV,
            StartTime = startTime,
            EndTime = endTime
        };

        _logger.LogInformation("LogOn:{logOn} Parameters:{parameters}", DateTime.Now.ToString("u"), parameters);

        return _messageSender
            .PublishAsync(
            subject: "gp.gamerecord.schedule.wgvia",
            data: Encoding.UTF8.GetBytes(JsonSerializer.Serialize(parameters)),
            cancellationToken: cancellationToken);
    }
}

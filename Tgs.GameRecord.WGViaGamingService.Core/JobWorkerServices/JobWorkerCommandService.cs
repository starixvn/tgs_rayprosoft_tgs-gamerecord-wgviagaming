using Microsoft.Extensions.Options;
using Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGamingService.Core.JobWorkerServices;

internal class JobWorkerCommandService : ICommandService<JobWorkerCommand>
{
    private readonly GameRecordInfoOptions _gameRecordInfoOptions;
    private readonly double _intervalDays;
    private readonly ICommandService<PullRecordCommand> _service;

    public JobWorkerCommandService(
        IOptions<GameRecordInfoOptions> options,
        ICommandService<PullRecordCommand> service)
    {
        _gameRecordInfoOptions = options.Value;
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _intervalDays =
            (_gameRecordInfoOptions.IntervalDaysMax < _gameRecordInfoOptions.IntervalDays)
            ? _gameRecordInfoOptions.IntervalDaysMax
            : (_gameRecordInfoOptions.IntervalDays < _gameRecordInfoOptions.IntervalDaysMinimum)
            ? _gameRecordInfoOptions.IntervalDaysMinimum
            : _gameRecordInfoOptions.IntervalDays;
    }

    public async ValueTask ExecuteAsync(JobWorkerCommand command, CancellationToken cancellationToken = default)
    {
        var endTime = DateTimeOffset.ParseExact(DateTimeOffset.UtcNow.ToString(@"yyyy-MM-dd\THH:mm:00K"), @"yyyy-MM-dd\THH:mm:ssK", null, styles: System.Globalization.DateTimeStyles.RoundtripKind);
        var startTime = endTime.AddDays(_intervalDays * -1);

        await _service
          .ExecuteAsync(
            command: new PullRecordCommand(
                StartTime: startTime,
                EndTime: endTime),
            cancellationToken: cancellationToken)
          .ConfigureAwait(false);
    }
}

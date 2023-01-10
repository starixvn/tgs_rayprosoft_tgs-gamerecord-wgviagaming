using Tgs.GameRecordSchedules.Protos;

namespace Tgs.GameRecord.WGViaGamingService.Core;

public interface IGameRecordSchedule
{
    ValueTask<GameRecordScheduleReply?> GetAsync(CancellationToken cancellationToken = default);

    ScheduleConfig GetConfig(GameRecordScheduleReply gameProviderSchedule);

    ValueTask<GameRecordScheduleReply?> InitAsync(Guid gameProviderId, CancellationToken cancellationToken = default);

    ValueTask<GameRecordScheduleReply?> UpdateAsync(GameRecordScheduleReply gameProviderSchedule, DateTimeOffset? lastGameRecordTime, DateTimeOffset sysTime, CancellationToken cancellationToken = default);
}

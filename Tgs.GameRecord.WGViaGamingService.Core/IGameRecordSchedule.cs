using Tgs.GameRecordSchedules.Protos;
using Tgs.GameRecord.WGViaGamingService.Core;

namespace Tgs.GameRecord.WGViaGamingService.Core
{
    public interface IGameRecordSchedule
    {
        ValueTask<GameRecordScheduleReply?> GetAsync(CancellationToken cancellationToken = default);

        ScheduleConfig GetConfig(GameRecordScheduleReply gameProviderSchedule);

        ValueTask<GameRecordScheduleReply?> InitAsync(Guid gameProviderId, CancellationToken cancellationToken = default);

        ValueTask<GameRecordScheduleReply?> UpdateAsync(GameRecordScheduleReply gameProviderSchedule, DateTimeOffset? lastGameRecorTime, DateTimeOffset sysTime, CancellationToken cancellationToken = default);
    }
}

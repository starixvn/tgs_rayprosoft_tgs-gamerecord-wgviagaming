namespace Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices;

public record PullRecordCommand(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);

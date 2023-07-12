namespace Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;

public record PullRecordCommand(
    Uri ApiUrl,
    string PartnerCode,
    string SecretKey,
    string SecretIV,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);

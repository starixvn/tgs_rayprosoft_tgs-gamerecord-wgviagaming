namespace Tgs.GameRecord.WGViaGaming.Common;

public record GameRecordMessage
{
    public Uri ApiUrl { get; init; } = default!;
    public string PartnerCode { get; init; } = default!;
    public string SecretKey { get; init; } = default!;
    public string SecretIV { get; init; } = default!;
    public DateTimeOffset StartTime { get; init; } = default!;

    public DateTimeOffset EndTime { get; init; } = default!;
}
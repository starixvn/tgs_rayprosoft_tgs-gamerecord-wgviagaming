namespace Tgs.GameRecord.WGViaGaming.ViewModels;

public record GameRecordQueryViewModel
{
	public Uri ApiUrl { get; init; } = default!;
	public string PartnerCode { get; init; } = default!;
	public string PartnerKey { get; init; } = default!;
	public string PartnerIV { get; init; } = default!;
	public DateTimeOffset StartTime { get; init; } = default!;
	public DateTimeOffset EndTime { get; init; } = default!;
}

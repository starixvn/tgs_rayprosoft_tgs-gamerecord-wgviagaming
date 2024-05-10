namespace Tgs.GameRecord.WGViaGaming.Core;

public record GameRecordInfoOptions
{
	public string GameProviderCode { get; set; } = default!;
	public string GameProviderConfiguration { get; set; } = default!;
	public string GameExternalId { get; set; } = default!;
	public string HangfirePrefix { get; set; } = "gamerecord-wgvia-hangfire:";
	public string HangfireLocking { get; set; } = "gamerecord-wgvia-locking:";
	public double IntervalDays { get; set; } = 2;
	public double IntervalDaysMax { get; set; } = 30;
	public double IntervalDaysMinimum { get; set; } = 1;
}

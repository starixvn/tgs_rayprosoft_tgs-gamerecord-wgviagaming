namespace Tgs.GameRecord.WGViaGamingService.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public double IntervalDays { get; set; } = 2;
    public double IntervalDaysMax { get; set; } = 30;
    public double IntervalDaysMinimum { get; set; } = 1;
}

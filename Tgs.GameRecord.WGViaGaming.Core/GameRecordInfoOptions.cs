namespace Tgs.GameRecord.WGViaGaming.Core;

public record GameRecordInfoOptions
{
    public string GameProviderCode { get; set; } = default!;
    public string GameProviderConfiguration { get; set; } = default!;
    public string GameExternalId { get; set; } = default!;
    public string GameProviderHangfire { get; set; } = default!;
    public double IntervalDays { get; set; } = 2;
    public double IntervalDaysMax { get; set; } = 30;
    public double IntervalDaysMinimum { get; set; } = 1;
}

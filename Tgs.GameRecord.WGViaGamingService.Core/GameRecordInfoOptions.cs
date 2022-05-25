namespace Tgs.GameRecord.WGViaGamingService.Core
{
    public record GameRecordInfoOptions
    {
        public string GameProviderCode { get; set; } = default!;
        public string GameProviderConfiguration { get; set; } = default!;
        public int IntervalDayMax { get; set; } = 30;
        public int IntervalDay { get; set; } = 2;
    }
}

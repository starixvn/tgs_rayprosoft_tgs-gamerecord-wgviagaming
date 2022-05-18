namespace Tgs.GameRecord.WGViaGamingService
{
    public record WGViaGamingMessageModel
    {
        public string GameProviderCode { get; init; } = default!;
        public DateTimeOffset SendDateTime { get; init; }
    }
}

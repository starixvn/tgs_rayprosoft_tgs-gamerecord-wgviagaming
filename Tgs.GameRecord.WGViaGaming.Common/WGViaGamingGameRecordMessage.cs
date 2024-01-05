using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.WGViaGaming.Common;

public record WGViaGamingGameRecordMessage
{
    [Required]
    [DataType(DataType.Url)]
    public Uri ApiUrl { get; init; } = default!;
    [Required]
    public string PartnerCode { get; init; } = default!;
    [Required]
    public string SecretKey { get; init; } = default!;
    [Required]
    public string SecretIV { get; init; } = default!;
    [Required]
    [DataType(DataType.DateTime)]
    public DateTimeOffset StartTime { get; init; } = default!;
    [Required]
    [DataType(DataType.DateTime)]
    public DateTimeOffset EndTime { get; init; } = default!;
}

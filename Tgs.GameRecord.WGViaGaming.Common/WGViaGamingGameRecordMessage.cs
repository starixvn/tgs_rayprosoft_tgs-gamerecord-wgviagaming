using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tgs.GameRecord.WGViaGaming.Common;

public record WGViaGamingGameRecordMessage
{
	[Required]
	[DataType(DataType.Url)]
	[DefaultValue(typeof(Uri), "https://via.itd888.com/")]
	public Uri ApiUrl { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "PT")]
	public string PartnerCode { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "a888888888888888")]
	public string SecretKey { get; init; } = default!;
	[Required]
	[DefaultValue(typeof(string), "b888888888888888")]
	public string SecretIV { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-06-28T00:00:00+08:00")]
	public DateTimeOffset StartTime { get; init; } = default!;
	[Required]
	[DataType(DataType.DateTime)]
	[DefaultValue(typeof(DateTimeOffset), "2023-06-29T00:00:00+08:00")]
	public DateTimeOffset EndTime { get; init; } = default!;
}

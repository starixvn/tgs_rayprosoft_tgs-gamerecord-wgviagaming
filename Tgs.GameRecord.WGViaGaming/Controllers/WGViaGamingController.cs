using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;
using Tgs.GameRecord.WGViaGaming.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGaming.Controllers;

[ApiController]
[Route("WGVIA")]
public class WGViaGamingController : ControllerBase
{
	/// <summary>
	/// Games the record asynchronous.
	/// </summary>
	/// <param name="service">The service.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	/// <remarks>
	/// <b>Example</b><br />
	///
	///     Post
	///
	///         {
	///             "apiUrl": "https://via.itd888.com/",
	///             "partnerCode": "PT",
	///             "partnerKey": "a888888888888888",
	///             "partnerIV": "b888888888888888",
	///             "startTime": "2023-06-28T00:00:00+08:00",
	///             "endTime": "2023-06-29T00:00:00+08:00"
	///         }
	/// </remarks>
	[HttpPost]
	public ValueTask GameRecordAsync(
		[FromServices] ICommandService<PullRecordCommand> service,
		GameRecordQueryViewModel source)
	{
		var command = new PullRecordCommand(
			ApiUrl: source.ApiUrl,
			PartnerCode: source.PartnerCode,
			SecretKey: source.PartnerKey,
			SecretIV: source.PartnerIV,
			StartTime: source.StartTime,
			EndTime: source.EndTime);

		return service
			.ExecuteAsync(
			command: command,
			cancellationToken: HttpContext.RequestAborted);
	}
}

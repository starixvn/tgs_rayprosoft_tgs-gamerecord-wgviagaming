using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.WGViaGaming.Common;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.WGViaGaming.Controllers;

[ApiController]
[Route("WGVIA")]
public class WGViaGamingController : ControllerBase
{
	/// <summary>
	/// Games the record manual asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("manual")]
	public ValueTask GameRecordManualAsync(
		[FromServices] IMessageSender messageSender,
		WGViaGamingGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: WGViaGamingNatsConst.ManualPublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);

	/// <summary>
	/// Games the record schedule asynchronous.
	/// </summary>
	/// <param name="messageSender">The message sender.</param>
	/// <param name="source">The source.</param>
	/// <returns></returns>
	[HttpPost("schedule")]
	public ValueTask GameRecordScheduleAsync(
		[FromServices] IMessageSender messageSender,
		WGViaGamingGameRecordMessage source)
		=> messageSender.PublishAsync(
			subject: WGViaGamingNatsConst.SchedulePublish,
			data: JsonSerializer.SerializeToUtf8Bytes(source),
			cancellationToken: HttpContext.RequestAborted);
}

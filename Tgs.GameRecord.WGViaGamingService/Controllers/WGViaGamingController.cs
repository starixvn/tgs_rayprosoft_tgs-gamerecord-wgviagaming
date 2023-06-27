using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices;
using Tgs.GameRecord.WGViaGamingService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGamingService.Controllers;

[ApiController]
[Route("WGVIA")]
public class WGViaGamingController : ControllerBase
{
    [HttpPost]
    public async ValueTask<GameRecordResultViewModel> GameRecordAsync(
        [FromServices] ICommandService<PullRecordCommand> service,
        GameRecordQueryViewModel model)
    {
        await service
            .ExecuteAsync(
            command: new PullRecordCommand(
                StartTime: model.StartTime,
                EndTime: model.EndTime),
            cancellationToken: HttpContext.RequestAborted)
            .ConfigureAwait(false);

        return new GameRecordResultViewModel()
        {
            Success = true
        };
    }
}

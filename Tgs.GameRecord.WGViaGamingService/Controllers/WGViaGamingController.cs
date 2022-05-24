using Microsoft.AspNetCore.Mvc;
using Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices;
using Tgs.GameRecord.WGViaGamingService.ViewModels;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGamingService.Controllers
{
    [ApiController]
    [Route("WGVIA")]
    public class WGViaGamingController : ControllerBase
    {
        [HttpPost]
        public async ValueTask<GameRecordResultViewModel> GameRecordAsync(
            [FromServices] IGetService<GameRecordQuery, GameRecordResult> service,
            GameRecordQueryViewModel model)
        {
            var result = await service.GetAsync(new GameRecordQuery()
            {
                StartTime = model.StartTime,
                EndTime = model.EndTime
            }).ConfigureAwait(false);

            return new GameRecordResultViewModel()
            {
                Success = result.Success,
                LastGameRecordTime = result.LastGameRecordTime,
                SuccessCount = result.SuccessCount,
                FailureCount = result.FailureCount,
                Count = result.Count,
            };
        }
    }
}

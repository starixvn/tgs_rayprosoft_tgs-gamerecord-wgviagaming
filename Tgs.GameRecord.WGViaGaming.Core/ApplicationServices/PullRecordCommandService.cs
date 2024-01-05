using Microsoft.Extensions.Logging;
using Tgs.GameInterface.WGViaGaming.Client;
using Tgs.GameInterface.WGViaGaming.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;

internal class PullRecordCommandService(
    ILogger<PullRecordCommandService> logger,
    IGameInterfaceWGViaGamingClient client,
    ICommandService<HandleRecordCommand> service) : ICommandService<PullRecordCommand>
{
    public async ValueTask ExecuteAsync(PullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var page = 1;
        var rows = 1000;
        IEnumerable<GameRecordInfo> gameRecords;
        do
        {
            gameRecords = await client.GameRecordAsync(
                apiUrl: command.ApiUrl,
                partnerCode: command.PartnerCode,
                secretKey: command.SecretKey,
                secretIV: command.SecretIV,
                startTime: command.StartTime,
                endTime: command.EndTime,
                pageIndex: page,
                pageSize: rows,
                cancellationToken: cancellationToken)
                .ToArrayAsync(
                cancellationToken: cancellationToken)
                .ConfigureAwait(false)
                ?? Enumerable.Empty<GameRecordInfo>();

            foreach (var gameRecord in gameRecords)
            {
                try
                {
                    await service.ExecuteAsync(
                        command: new HandleRecordCommand(gameRecord),
                        cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "LogOn:{logOn} Message: An exception occurred in this game record.({gameRecord})", DateTime.Now.ToString("o"), gameRecord);
                }
            }

            page++;
        } while (gameRecords.Any());
    }
}

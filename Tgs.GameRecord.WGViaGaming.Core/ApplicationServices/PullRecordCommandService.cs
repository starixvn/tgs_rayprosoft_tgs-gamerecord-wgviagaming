using Microsoft.Extensions.Logging;
using Tgs.GameInterface.WGViaGaming.Client;
using Tgs.GameInterface.WGViaGaming.Protos;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;

internal class PullRecordCommandService : ICommandService<PullRecordCommand>
{
    private readonly IGameInterfaceWGViaGamingClient _client;
    private readonly ILogger<PullRecordCommandService> _logger;
    private readonly ICommandService<HandleRecordCommand> _service;

    public PullRecordCommandService(
        ILogger<PullRecordCommandService> logger,
        IGameInterfaceWGViaGamingClient client,
        ICommandService<HandleRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask ExecuteAsync(PullRecordCommand command, CancellationToken cancellationToken = default)
    {
        var page = 1;
        var rows = 1000;
        while (true)
        {
            var gameRecords = await _client.GameRecordAsync(
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

            if (!gameRecords.Any())
            {
                break;
            }

            page++;

            foreach (var gameRecord in gameRecords)
            {
                try
                {
                    await _service
                    .ExecuteAsync(
                         command: new HandleRecordCommand(gameRecord),
                         cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "遊戲紀錄發生例外.({data})", gameRecord);
                }
            }
        }
    }
}

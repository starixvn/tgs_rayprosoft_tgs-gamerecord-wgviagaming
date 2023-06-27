using Microsoft.Extensions.Logging;
using Tgs.GameProviders.Client;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices;

internal class PullRecordCommandService : ICommandService<PullRecordCommand>
{
    private readonly IGameRecordListClient _client;
    private readonly ILogger<PullRecordCommandService> _logger;
    private readonly ICommandService<HandleRecordCommand> _service;

    public PullRecordCommandService(
        ILogger<PullRecordCommandService> logger,
        IGameRecordListClient client,
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
            var response = await _client.WGHorseGameRecordListAsync(
                startTime: command.StartTime,
                endTime: command.EndTime,
                page: page,
                rows: rows,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            if (response == null || !response.Success)
            {
                break;
            }

            var datas = response.Datas ?? Enumerable.Empty<GameRecordModle>();

            if (!datas.Any())
            {
                break;
            }

            page++;

            foreach (var data in datas)
            {
                try
                {
                    await _service
                    .ExecuteAsync(
                         command: new HandleRecordCommand(data),
                         cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "已結算遊戲紀錄發生例外.({data})", data);
                }
            }
        }
    }
}

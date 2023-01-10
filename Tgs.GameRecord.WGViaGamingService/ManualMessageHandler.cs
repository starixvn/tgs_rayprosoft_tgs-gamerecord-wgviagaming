using System.Text;
using System.Text.Json;
using Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGamingService;

public class ManualMessageHandler : IMessageProcessor
{
    private readonly IGetService<GameRecordQuery, GameRecordResult> _service;

    public ManualMessageHandler(IGetService<GameRecordQuery, GameRecordResult> service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var request = JsonSerializer.Deserialize<ManualMessageRequest>(json)!;

        var result = await _service.GetAsync(new GameRecordQuery()
        {
            StartTime = request.StartTime,
            EndTime = request.EndTime
        }, cancellationToken).ConfigureAwait(false);

        var response = new ManualMessageResponse()
        {
            Success = result.Success,
            LastGameRecordTime = result.LastGameRecordTime,
            SuccessCount = result.SuccessCount,
            FailureCount = result.FailureCount,
            Count = result.Count
        };

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
    }
}

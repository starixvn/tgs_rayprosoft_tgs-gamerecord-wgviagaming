using System.Text;
using System.Text.Json;
using Tgs.GameRecord.WGViaGamingService.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGamingService;

public class ManualMessageHandler : IMessageProcessor
{
    private readonly ICommandService<PullRecordCommand> _service;

    public ManualMessageHandler(
        ICommandService<PullRecordCommand> service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var request = JsonSerializer.Deserialize<ManualMessageRequest>(json)!;

        await _service
          .ExecuteAsync(
            command: new PullRecordCommand(
                StartTime: request.StartTime,
                EndTime: request.EndTime),
            cancellationToken: cancellationToken)
          .ConfigureAwait(false);

        var response = new ManualMessageResponse()
        {
            Success = true
        };

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
    }
}

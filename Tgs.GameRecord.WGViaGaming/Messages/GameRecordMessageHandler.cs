using System.Text;
using System.Text.Json;
using Tgs.GameRecord.WGViaGaming.Common;
using Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;
using Tgs.MessageQueue;
using Tgs.ServiceUtility;

namespace Tgs.GameRecord.WGViaGaming.Messages;

public class GameRecordMessageHandler : IMessageHandler
{
    private readonly ILogger<GameRecordMessageHandler> _logger;
    private readonly ICommandService<PullRecordCommand> _service;

    public GameRecordMessageHandler(
        ILogger<GameRecordMessageHandler> logger,
        ICommandService<PullRecordCommand> service)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<GameRecordMessage>(json)
            ?? throw new NullReferenceException("The request data resolves to a null value");

        _logger.LogInformation("LogOn:{logOn} Information Request:{message}", DateTime.Now.ToString("o"), message);

        var command = new PullRecordCommand(
            ApiUrl: message.ApiUrl,
            PartnerCode: message.PartnerCode,
            SecretKey: message.SecretKey,
            SecretIV: message.SecretIV,
            StartTime: message.StartTime,
            EndTime: message.EndTime);

        return _service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}

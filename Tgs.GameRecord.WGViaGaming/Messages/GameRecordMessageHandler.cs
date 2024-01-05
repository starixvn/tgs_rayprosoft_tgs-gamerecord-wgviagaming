using System.Text;
using System.Text.Json;
using Tgs.GameRecord.WGViaGaming.Common;
using Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.GameRecord.WGViaGaming.Messages;

public class GameRecordMessageHandler(
    ILogger<GameRecordMessageHandler> logger,
    ICommandService<PullRecordCommand> service) : IMessageHandler
{
    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var json = Encoding.UTF8.GetString(data.Span);
        var message = JsonSerializer.Deserialize<WGViaGamingGameRecordMessage>(json)
            ?? throw new NullReferenceException("The request data resolves to a null value");

        logger.LogInformation("LogOn:{logOn} Information Request:{message}", DateTime.Now.ToString("o"), message);

        var command = new PullRecordCommand(
            ApiUrl: message.ApiUrl,
            PartnerCode: message.PartnerCode,
            SecretKey: message.SecretKey,
            SecretIV: message.SecretIV,
            StartTime: message.StartTime,
            EndTime: message.EndTime);

        return service.ExecuteAsync(
            command: command,
            cancellationToken: cancellationToken);
    }
}

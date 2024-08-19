using Tgs.GameRecord.WGViaGaming.Core.ApplicationServices;

namespace Tgs.GameRecord.WGViaGaming.Core;

public interface IPullRecordDispatcher
{
	ValueTask PushCommandAsync(PullRecordCommand command, CancellationToken cancellationToken = default);
}

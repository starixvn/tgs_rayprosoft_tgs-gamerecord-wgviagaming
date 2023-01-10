namespace Tgs.GameRecord.WGViaGamingService.Core;

public interface ISystemClock
{
    DateTimeOffset CurrentTime { get; }
}

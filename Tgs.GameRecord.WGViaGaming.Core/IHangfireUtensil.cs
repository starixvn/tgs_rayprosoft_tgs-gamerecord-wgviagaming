using Tgs.GameRecord.WGViaGaming.Core.JobApplicationServices;

namespace Tgs.GameRecord.WGViaGaming.Core;

public interface IHangfireUtensil
{
	public Task JobCheckAsync();

	public Task JobExecuteAsync(JobExecuteCommand source);

	public Task JobInitAsync();
}

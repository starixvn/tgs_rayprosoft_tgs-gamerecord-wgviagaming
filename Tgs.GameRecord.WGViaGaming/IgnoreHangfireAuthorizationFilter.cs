using Hangfire.Dashboard;

namespace Tgs.GameRecord.WGViaGaming;

public class IgnoreHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context) => true;
}

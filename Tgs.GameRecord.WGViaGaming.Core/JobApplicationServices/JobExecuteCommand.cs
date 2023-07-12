namespace Tgs.GameRecord.WGViaGaming.Core.JobApplicationServices;

public record JobExecuteCommand(
    Uri ApiUrl,
    string PartnerCode,
    string SecretKey,
    string SecretIV);

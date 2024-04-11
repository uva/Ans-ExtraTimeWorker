using Microsoft.Extensions.Options;
using UvA.SIS.QueryClient;

namespace UvA.Ans.ExtraTimeWorker;

public class Worker(IOptions<SisConfig> sisOptions, ILogger<Worker> logger, IHostApplicationLifetime applicationLifetime, 
    AnsClientFactory ansClientFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ansClient = ansClientFactory.CreateClient();
        var sisConfig = sisOptions.Value;
        var qasClient = new QASClient(sisConfig.Endpoint, sisConfig.Username, sisConfig.Password);
        
        var result = await qasClient.ExecuteQuery("Q_OH_AAN_EXTRA_TIJD",
            new Dictionary<string, string>());

        var targetStudentIds = result.Select(r => r.First().Value).ToArray();

        var currentUsers = (await ansClient.FindUsers("extra_time:true"))
            .Where(s => s.StudentNumber != null).ToDictionary(s => s.StudentNumber!);
        
        var toAdd = targetStudentIds
            .Where(s => currentUsers.GetValueOrDefault(s)?.HasExtraTime != true)
            .Select(s => s + "@uva.nl").ToArray();
        var targets = await ansClient.FindUsersByExternalId(toAdd);
        foreach (var tar in targets.Values)
        {
            logger.LogInformation($"Adding extra time to {tar.Id} ({tar.StudentNumber})");
            await ansClient.UpdateUser(tar.Id, true);
        }

        var toRemove = currentUsers.Values.Where(u => !targetStudentIds.Contains(u.StudentNumber)).ToArray();
        foreach (var tar in toRemove)
        {
            logger.LogInformation($"Removing extra time from {tar.Id} ({tar.StudentNumber})");
            await ansClient.UpdateUser(tar.Id, false);
        }

        applicationLifetime.StopApplication();
    }
}
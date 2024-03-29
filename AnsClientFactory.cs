using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using UvA.DataNose.Connectors.AnsConnector;

namespace UvA.Ans.ExtraTimeWorker;

public class AnsClientFactory(IOptions<AnsConfig> options, ILogger<AnsClient> logger)
{
    public AnsClient CreateClient()
    {
        var ansSettings = options.Value;
        var client = new HttpClient();
        client.BaseAddress = new Uri($"{ansSettings.Url}/api/v2/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ansSettings.Token);
        return new AnsClient(client, ansSettings.SchoolId, logger);
    }
}
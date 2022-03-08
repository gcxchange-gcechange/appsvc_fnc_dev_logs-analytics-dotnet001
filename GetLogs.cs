using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Azure;
using Microsoft.Extensions.Configuration;

namespace logs_analytics
{

    public static class GetLogs
    {


        [FunctionName("GetLogs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            IConfiguration config = new ConfigurationBuilder()

            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            log.LogInformation("C# HTTP trigger function processed a request.");
            string clientSecret = config["clientSecret"];
            string clientId = config["clientId"];
            string tenantid = config["tenantid"];
            string workspaceId = config["workspaceId"];

            ClientSecretCredential cred = new ClientSecretCredential(tenantid,clientId, clientSecret);
            bool Error = true;
            var client = new LogsQueryClient(cred);
            try
            {
                Response<LogsQueryResult> response = await client.QueryWorkspaceAsync(
                    workspaceId,
                    "SigninLogs | where TimeGenerated > ago(1d)",
                    new QueryTimeRange(TimeSpan.FromDays(1)));

                LogsTable table = response.Value.Table;

                foreach (var row in table.Rows)
                {
                    log.LogInformation(row["UserDisplayName"] + " " + row["UserType"]);
                }
                Error = false;
            }
            catch(Exception ex)
            {
                log.LogInformation(ex.Message);

            }
           
            string responseMessage = Error
                ? "Error gettings logs."
                : "Successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}

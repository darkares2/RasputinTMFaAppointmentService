using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;

namespace Rasputin.TM
{
    public static class GetUser
    {
        [FunctionName("GetUser")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
                                                    [Table("tblUsers")] CloudTable tblUser,
                                                    ILogger log)
        {
            log.LogInformation("GetUser called");

            Guid userID = Guid.Parse(req.Query["userID"].ToString());            
            User user = await new UserService().FindUser(log, tblUser, userID);
            if (user == null) {
                return new NotFoundResult();
            }
            string responseMessage = JsonConvert.SerializeObject(user);

            return new OkObjectResult(responseMessage);
        }
    }
}

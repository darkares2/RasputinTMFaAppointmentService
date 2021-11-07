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
    public static class CreateUser
    {
        [FunctionName("CreateUser")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
                                                    [Table("tblUsers")] CloudTable tblUser,
                                                    ILogger log)
        {
            log.LogInformation("CreateUser called");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = data?.name;
            string password = data?.password;
            User.UserTypes type = data?.type;

            User user = await new UserService().InsertUser(log, tblUser, name, password, type);

            string responseMessage = JsonConvert.SerializeObject(user);
            return new OkObjectResult(responseMessage);
        }
    }
}

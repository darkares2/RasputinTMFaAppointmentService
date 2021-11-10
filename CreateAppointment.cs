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
    public static class CreateAppointment
    {
        [FunctionName("CreateAppointment")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
                                                    [Queue("appointmentFromSlotqueue")] IAsyncCollector<string> appointmentFromSlotqueue,
                                                    ILogger log)
        {
            log.LogInformation("CreateAppointment called");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AppointmentCreateRequest data = (AppointmentCreateRequest)JsonConvert.DeserializeObject(requestBody, typeof(AppointmentCreateRequest));
            await appointmentFromSlotqueue.AddAsync(JsonConvert.SerializeObject(data));
            
            return new OkObjectResult(JsonConvert.SerializeObject(data));
        }
    }
}

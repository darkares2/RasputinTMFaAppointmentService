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
    public static class CloseAppointment
    {
        [FunctionName("CloseAppointment")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
                                                    [Table("tblAppointments")] CloudTable tblAppointment,
                                                    ILogger log)
        {
            log.LogInformation("CloseAppointment called");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AppointmentCloseRequest data = (AppointmentCloseRequest)JsonConvert.DeserializeObject(requestBody, typeof(AppointmentCloseRequest));
            Appointment appointment = await new AppointmentService().CloseAppointment(log, tblAppointment, data.AppointmentID);

            return new OkObjectResult(JsonConvert.SerializeObject(appointment));
        }
    }
}

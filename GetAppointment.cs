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
    public static class GetAppointment
    {
        [FunctionName("GetAppointment")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
                                                    [Table("tblAppointments")] CloudTable tblAppointment,
                                                    ILogger log)
        {
            log.LogInformation("GetAppointment called");

            string responseMessage = null;
            string userIDString = req.Query["UserID"].ToString();
            if (userIDString != null && !userIDString.Equals("")) {
                Appointment[] appointments = await new AppointmentService().FindUserAppointments(log, tblAppointment, Guid.Parse(userIDString));
                responseMessage = JsonConvert.SerializeObject(appointments);                
            } else {
                Guid AppointmentID = Guid.Parse(req.Query["AppointmentID"].ToString());            
                Appointment Appointment = await new AppointmentService().FindAppointment(log, tblAppointment, AppointmentID);
                if (Appointment == null) {
                    return new NotFoundResult();
                }
                responseMessage = JsonConvert.SerializeObject(Appointment);
            }

            return new OkObjectResult(responseMessage);
        }
    }
}

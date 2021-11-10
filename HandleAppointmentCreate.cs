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
    public static class HandleAppointmentFromSlot
    {
        [FunctionName("HandleAppointmentFromSlot")]
        public static async Task Run([QueueTrigger("appointmentCreateQueue")] string appointmentCreateQueueItem,
                                     [Queue("userMessageQueue")] IAsyncCollector<string> userMessageQueue,
                                     [Table("tblAppointments")] CloudTable tblAppointment,
                                     ILogger log)
        {
            log.LogInformation("HandleAppointmentFromSlot called");
            
            dynamic data = JsonConvert.DeserializeObject(appointmentCreateQueueItem);
            try {
                Appointment appointment = await new AppointmentService().InsertAppointment(log, tblAppointment, data?.Timeslot, data?.UserID, data?.SlotUserID, data?.ServiceID);
                if (appointment.AppointmentID != null) {
                    await userMessageQueue.AddAsync(JsonConvert.SerializeObject(new UserMessage(data?.UserID, "You have a new appointment, please login to RasputinTM and see the details")));
                } else {
                    await userMessageQueue.AddAsync(JsonConvert.SerializeObject(new UserMessage(data?.UserID, "Appointment creation failed")));
                }
            } catch(Exception ex) {
                await userMessageQueue.AddAsync(JsonConvert.SerializeObject(new UserMessage(data?.UserID, "Appointment creation failed: " + ex.Message)));
            }
        }
    }
}

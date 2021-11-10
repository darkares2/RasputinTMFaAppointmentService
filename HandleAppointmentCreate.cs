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
    public static class HandleAppointmentCreate
    {
        [FunctionName("HandleAppointmentCreate")]
        public static async Task Run([QueueTrigger("appointmentCreateQueue")] string appointmentCreateQueueItem,
                                     [Queue("userMessageQueue")] IAsyncCollector<string> userMessageQueue,
                                     [Table("tblAppointments")] CloudTable tblAppointment,
                                     ILogger log)
        {
            log.LogInformation("HandleAppointmentCreate called");
            
            dynamic data = JsonConvert.DeserializeObject(appointmentCreateQueueItem);
            DateTime timeslot = data?.Timeslot;
            Guid userID = data?.UserID;
            Guid slotUserID = data?.SlotUserID;
            Guid serviceID = data?.ServiceID;
            try {
                log.LogInformation($"Inserting: {timeslot}, {userID}, {slotUserID}, {serviceID}");
                Appointment appointment = await new AppointmentService().InsertAppointment(log, tblAppointment, timeslot, userID, slotUserID, serviceID);
                log.LogInformation($"Inserted: {appointment.AppointmentID}");
                if (appointment.AppointmentID != null) {
                    await userMessageQueue.AddAsync(JsonConvert.SerializeObject(new UserMessage(userID, "You have a new appointment, please login to RasputinTM and see the details")));
                } else {
                    await userMessageQueue.AddAsync(JsonConvert.SerializeObject(new UserMessage(userID, "Appointment creation failed")));
                }
            } catch(Exception ex) {
                await userMessageQueue.AddAsync(JsonConvert.SerializeObject(new UserMessage(userID, "Appointment creation failed: " + ex.Message)));
            }
        }
    }
}

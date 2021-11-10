using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace Rasputin.TM {
    public class AppointmentService {
        public async Task<Appointment> InsertAppointment(ILogger log, CloudTable tblAppointment, DateTime timeslot, Guid userID, Guid slotUserID, Guid serviceID)
        {
            Appointment Appointment = new Appointment(timeslot, userID, slotUserID, serviceID);
            TableOperation operation = TableOperation.Insert(Appointment);
            await tblAppointment.ExecuteAsync(operation);
            return Appointment;
        }

        public async Task<Appointment> FindAppointment(ILogger log, CloudTable tblAppointment, Guid AppointmentID)
        {
            string pk = "p1";
            string rk = AppointmentID.ToString();
            log.LogInformation($"FindAppointment: {pk},{rk}");
            TableOperation operation = TableOperation.Retrieve(pk, rk);
            try {
                return (Appointment)await tblAppointment.ExecuteAsync(operation);
            } catch(Exception ex) {
                log.LogWarning(ex, "FindAppointment", AppointmentID);
                return null;
            }
        }
    }
}
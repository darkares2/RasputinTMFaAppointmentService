using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace Rasputin.TM {
    public class AppointmentService {
        public async Task<Appointment> InsertAppointment(ILogger log, CloudTable tblAppointment, DateTime timeslot, Guid userID, Guid slotUserID, Guid serviceID)
        {
            Appointment appointment = new Appointment(timeslot, userID, slotUserID, serviceID);
            TableOperation operation = TableOperation.Insert(appointment);
            await tblAppointment.ExecuteAsync(operation);
            return appointment;
        }

        public async Task<Appointment[]> FindUserAppointments(ILogger log, CloudTable tblAppointment, Guid userID, bool open)
        {
            log.LogInformation($"FindUserAppointments by user {userID}");
            List<Appointment> result = new List<Appointment>();
            TableQuery<Appointment> query = new TableQuery<Appointment>().Where(TableQuery.GenerateFilterConditionForGuid("UserID", QueryComparisons.Equal, userID));
            TableContinuationToken continuationToken = null;
            try {
                do {
                var page = await tblAppointment.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = page.ContinuationToken;
                result.AddRange(page.Results);
                } while(continuationToken != null);
                return result.Where(x => x.Open == open).OrderBy(x => x.Timeslot).ToArray();
            } catch(Exception ex) {
                log.LogWarning(ex, "FindUserAppointments");
                return null;
            }
        }

        public async Task<Appointment> CloseAppointment(ILogger log, CloudTable tblAppointment, Guid appointmentID)
        {
            Appointment appointment = await FindAppointment(log, tblAppointment, appointmentID);
            appointment.Open = false;
            TableOperation operation = TableOperation.Replace(appointment);
            await tblAppointment.ExecuteAsync(operation);
            return appointment;
        }

        public async Task<Appointment[]> FindSlotUserAppointments(ILogger log, CloudTable tblAppointment, Guid slotUserID, bool open)
        {
            log.LogInformation($"FindUserAppointments by user {slotUserID}");
            List<Appointment> result = new List<Appointment>();
            TableQuery<Appointment> query = new TableQuery<Appointment>().Where(TableQuery.GenerateFilterConditionForGuid("SlotUserID", QueryComparisons.Equal, slotUserID));
            TableContinuationToken continuationToken = null;
            try {
                do {
                var page = await tblAppointment.ExecuteQuerySegmentedAsync(query, continuationToken);
                continuationToken = page.ContinuationToken;
                result.AddRange(page.Results);
                } while(continuationToken != null);
                return result.Where(x => x.Open == open).OrderBy(x => x.Timeslot).ToArray();
            } catch(Exception ex) {
                log.LogWarning(ex, "FindSlotUserAppointments");
                return null;
            }
        }

        public async Task<Appointment> FindAppointment(ILogger log, CloudTable tblAppointment, Guid AppointmentID)
        {
            string pk = "p1";
            string rk = AppointmentID.ToString();
            log.LogInformation($"FindAppointment: {pk},{rk}");
            TableOperation operation = TableOperation.Retrieve(pk, rk);
            try {
                var tableResult = await tblAppointment.ExecuteAsync(operation);
                return tableResult.Result as Appointment != null ? tableResult.Result as Appointment : (Appointment)tableResult;
            } catch(Exception ex) {
                log.LogWarning(ex, "FindAppointment", AppointmentID);
                return null;
            }
        }
    }
}
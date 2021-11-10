
using System;
using Microsoft.Azure.Cosmos.Table;

namespace Rasputin.TM{
    public class Appointment : TableEntity {
        public Appointment(DateTime timslot, Guid userID, Guid slotUserID, Guid serviceID)
        {
            this.PartitionKey = "p1";
            this.RowKey = Guid.NewGuid().ToString();
            this.Timeslot = Timeslot;
            this.ServiceID = serviceID;
            this.UserID = userID;
            this.SlotUserID = slotUserID;
        }
        Appointment() { }
        public DateTime? Timeslot { get; set; }
        public Guid? UserID { get; set; }
        public Guid? SlotUserID { get; set; }
        public Guid? ServiceID { get; set; }
        public Guid AppointmentID { get { return Guid.Parse(RowKey); } }

        public static explicit operator Appointment(TableResult v)
        {
            DynamicTableEntity entity = (DynamicTableEntity)v.Result;
            Appointment AppointmentProfile = new Appointment();
            AppointmentProfile.PartitionKey = entity.PartitionKey;
            AppointmentProfile.RowKey = entity.RowKey;
            AppointmentProfile.Timestamp = entity.Timestamp;
            AppointmentProfile.ETag = entity.ETag;
            AppointmentProfile.Timeslot = entity.Properties["Timeslot"].DateTime;
            AppointmentProfile.UserID = entity.Properties["UserID"].GuidValue;
            AppointmentProfile.SlotUserID = entity.Properties["SlotUserID"].GuidValue;
            AppointmentProfile.ServiceID = entity.Properties["ServiceID"].GuidValue;

            return AppointmentProfile;
        }

    }
}
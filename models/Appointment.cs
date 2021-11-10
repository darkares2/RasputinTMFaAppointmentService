
using System;
using Microsoft.Azure.Cosmos.Table;

namespace Rasputin.TM{
    public class Appointment : TableEntity {
        public Appointment(DateTime scheduleTimestamp, Guid appointmentTypeID)
        {
            this.PartitionKey = "p1";
            this.RowKey = Guid.NewGuid().ToString();
            this.ScheduleTimestamp = scheduleTimestamp;
            this.Type = appointmentTypeID.ToString();
        }
        Appointment() { }
        public DateTime? ScheduleTimestamp { get; set; }
        public string Type { get; set; }
        public string Password { get; set; }
        public Guid TypeID { get { return Guid.Parse(Type); } }
        public Guid AppointmentID { get { return Guid.Parse(RowKey); } }

        public static explicit operator Appointment(TableResult v)
        {
            DynamicTableEntity entity = (DynamicTableEntity)v.Result;
            Appointment AppointmentProfile = new Appointment();
            AppointmentProfile.PartitionKey = entity.PartitionKey;
            AppointmentProfile.RowKey = entity.RowKey;
            AppointmentProfile.Timestamp = entity.Timestamp;
            AppointmentProfile.ETag = entity.ETag;
            AppointmentProfile.ScheduleTimestamp = entity.Properties["ScheduleTimestamp"].DateTime;
            AppointmentProfile.Type = entity.Properties["Type"].StringValue;

            return AppointmentProfile;
        }

    }
}
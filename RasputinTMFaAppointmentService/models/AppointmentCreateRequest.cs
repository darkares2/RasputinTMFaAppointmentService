using System;

namespace Rasputin.TM {
    public class AppointmentCreateRequest {
        public Guid SlotID {get;set;}
        public Guid UserID {get;set;}        
        public Guid ServiceID {get; set;}
    }
}
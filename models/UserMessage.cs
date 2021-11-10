using System;

namespace Rasputin.TM {
    public class UserMessage {
        public UserMessage(Guid userID, string message) {
            this.UserID = userID;
            this.message = message;
        }
        public Guid UserID { get; set;}
        public string message { get; set; }
    }
}

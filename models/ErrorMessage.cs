namespace Rasputin.TM {
    public class ErrorMessage {
        public ErrorMessage(int status, string statusText) {
            this.status = status;
            this.statusText = statusText;
        }
        public int status { get; set; }
        public string statusText { get; set;}
    }
}
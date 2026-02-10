namespace Its.PleaseProtect.Api.Services
{
    public class Alert
    {
        public string Status { get; set; }
        public Dictionary<string,string> Labels { get; set; }
        public Dictionary<string,string> Annotations { get; set; }
        public DateTime StartsAt { get; set; }

        public Alert()
        {
            Labels = new Dictionary<string, string>();
            Annotations = new Dictionary<string, string>();
            Status = "";
        }
    }

    public class AlertmanagerWebhook
    {
        public string Receiver { get; set; }
        public string Status { get; set; }
        public List<Alert> Alerts { get; set; }

        public AlertmanagerWebhook()
        {
            Alerts = new List<Alert>();
            Receiver = "";
            Status = "";
        }
    }
}
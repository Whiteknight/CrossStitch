namespace CrossStitch.Core.Messages.Alerts
{
    public enum AlertType
    {
        Warning,
        Error
    }

    public class AlertEvent
    {
        public string Key { get; set; }
        public AlertType Type { get; set; }
        public string Message { get; set; }
    }
}

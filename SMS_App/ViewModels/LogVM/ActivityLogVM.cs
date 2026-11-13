using System;

namespace SMS_App.ViewModels.LogVM;

public class ActivityLogVM
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageTemplate { get; set; }
    public string Exception { get; set; }
    public string Properties { get; set; }
}

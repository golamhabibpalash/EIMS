using System;

namespace SMS.Entities;

public class Log
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string MessageTemplate { get; set; }
    public string Exception { get; set; }
    public string Properties { get; set; }
}

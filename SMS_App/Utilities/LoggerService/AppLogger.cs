using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SMS_App.Utilities.LoggerService;

public interface IAppLogger
{
    Task InfoAsync(
        string message,
        string exception = "No Exception",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    );
    Task WarningAsync(
        string message,
        string exception = "No Exception",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    );

    Task ErrorAsync(
        string message,
        string exception,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    );
}

public class AppLogger : IAppLogger
{
    private readonly ILogManager _logManager;

    public AppLogger(ILogManager logManager)
    {
        _logManager = logManager;
    }

    public async Task InfoAsync(
        string message,
        string exception = "No Exception",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        await WriteLogAsync("Info", message, exception, filePath, lineNumber, memberName);
    }

    public async Task WarningAsync(
        string message,
        string exception = "No Exception",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        await WriteLogAsync("Warning", message, exception, filePath, lineNumber, memberName);
    }

    public async Task ErrorAsync(
        string message,
        string exception,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = ""
    )
    {
        await WriteLogAsync("Error", message, exception, filePath, lineNumber, memberName);
    }

    private async Task WriteLogAsync(
        string level,
        string message,
        string exception,
        string filePath,
        int lineNumber,
        string memberName
    )
    {
        await _logManager.AddAsync(new Log
        {
            Level = level,
            Exception = exception,
            MessageTemplate = $"{Path.GetFileNameWithoutExtension(filePath)}.{memberName} (line : {lineNumber})",
            Message = message,
            Timestamp = DateTime.Now
        });
    }
}
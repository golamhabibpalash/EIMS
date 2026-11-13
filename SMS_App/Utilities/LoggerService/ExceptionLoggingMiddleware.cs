using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SMS.BLL.Contracts;
using SMS.Entities;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SMS_App.Utilities.LoggerService;

/// <summary>
/// Records every unhandled exception in the Logs table so it is visible on /Logs,
/// then rethrows so the configured error page still renders as before.
/// </summary>
public class ExceptionLoggingMiddleware
{
    // A full stack trace is stored, but a runaway one should not bloat the table
    // or make the Logs page unreadable.
    private const int MaxExceptionLength = 20000;

    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExceptionLoggingMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
    {
        _next = next;
        _scopeFactory = scopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // A client that navigates away mid-request is not a fault worth recording.
            if (ex is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
            {
                throw;
            }

            await LogAsync(context, ex);
            throw;
        }
    }

    private async Task LogAsync(HttpContext context, Exception ex)
    {
        try
        {
            // A fresh scope, not the request's own: if the exception came out of EF the
            // request's DbContext may be unusable, and the log write would fail with it.
            using var scope = _scopeFactory.CreateScope();
            var logManager = scope.ServiceProvider.GetService<ILogManager>();
            if (logManager is null)
            {
                return;
            }

            var text = ex.ToString();
            if (text.Length > MaxExceptionLength)
            {
                text = text.Substring(0, MaxExceptionLength) + Environment.NewLine + "... (truncated)";
            }

            await logManager.AddAsync(new Log
            {
                Level = "Error",
                Timestamp = DateTime.Now,
                Message = ex.Message,
                MessageTemplate = $"{context.Request.Method} {context.Request.Path}{context.Request.QueryString}",
                Exception = text,
                Properties = JsonSerializer.Serialize(new
                {
                    User = context.User?.Identity?.IsAuthenticated == true
                        ? context.User.Identity.Name
                        : "anonymous",
                    Path = context.Request.Path.Value,
                    Method = context.Request.Method,
                    Query = context.Request.QueryString.Value,
                    Referer = context.Request.Headers["Referer"].ToString(),
                    RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                    TraceId = context.TraceIdentifier,
                    ExceptionType = ex.GetType().FullName
                })
            });
        }
        catch
        {
            // Logging must never replace the original failure. If the database is
            // unreachable the exception still has to reach the error page.
        }
    }
}

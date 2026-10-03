using System;
using System.Collections.Generic;

namespace SMS_App.Utilities.ShortMessageService
{
    /// <summary>
    /// Process-wide anti-duplicate gate for outgoing SMS. The PhoneSMS-table
    /// "already sent?" check is check-then-act: two overlapping Hangfire runs
    /// (a slow run overrunning its 10-minute interval, or a scheduled job
    /// racing the real-time punch notifier) can both pass it and send the
    /// same check-in SMS twice to one number. Claiming a (number, type, day)
    /// slot here makes the second sender skip before touching the gateway.
    ///
    /// Claims expire after 5 minutes, so a failed send is retried by the next
    /// scheduled run and a crashed thread can never suppress a number for the
    /// whole day. Same-day repeats are never legitimate (one SMS per number /
    /// type / day by design), so holding a slot briefly is always safe.
    /// </summary>
    public static class SmsSendGate
    {
        private static readonly object _lock = new();
        private static readonly Dictionary<string, DateTime> _claimed = new();
        private static readonly TimeSpan ClaimTtl = TimeSpan.FromMinutes(5);

        public static bool TryClaim(string mobileNumber, string smsType)
        {
            string key = $"{smsType?.Trim()}|{(mobileNumber ?? string.Empty).Trim()}|{DateTime.Today:yyyyMMdd}";
            lock (_lock)
            {
                if (_claimed.TryGetValue(key, out var claimedAt) && DateTime.UtcNow - claimedAt < ClaimTtl)
                    return false;
                _claimed[key] = DateTime.UtcNow;
                if (_claimed.Count > 2000)
                {
                    var cutoff = DateTime.UtcNow - ClaimTtl;
                    foreach (var k in new List<string>(_claimed.Keys))
                    {
                        if (_claimed[k] < cutoff)
                            _claimed.Remove(k);
                    }
                }
                return true;
            }
        }
    }
}

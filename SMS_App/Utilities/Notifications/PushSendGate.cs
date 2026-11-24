using System;
using System.Collections.Generic;

namespace SMS_App.Utilities.Notifications
{
    public static class PushSendGate
    {
        private static readonly object _lock = new();
        private static readonly Dictionary<string, DateTime> _claimed = new();
        private static readonly TimeSpan ClaimTtl = TimeSpan.FromMinutes(5);

        public static bool TryClaim(int studentId, DateTime date, string scope = null)
        {
            var key = $"{scope}|{studentId}|{date:yyyyMMdd}";
            lock (_lock)
            {
                if (_claimed.TryGetValue(key, out var claimedAt) && DateTime.UtcNow - claimedAt < ClaimTtl)
                {
                    return false;
                }

                _claimed[key] = DateTime.UtcNow;

                if (_claimed.Count > 2000)
                {
                    var cutoff = DateTime.UtcNow - ClaimTtl;
                    foreach (var item in new List<string>(_claimed.Keys))
                    {
                        if (_claimed[item] < cutoff)
                        {
                            _claimed.Remove(item);
                        }
                    }
                }

                return true;
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class PhoneSMSRepository : Repository<PhoneSMS>, IPhoneSMSRepository
    {
        public PhoneSMSRepository(ApplicationDbContext context):base(context)
        {
        }

        public async Task<bool> IsSMSSendForAttendance(string phoneNumber, string smsType, string dateTime)
        {
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(smsType) || string.IsNullOrEmpty(dateTime))
            {
                Exception ex = new Exception();
                throw ex;
            }
            try
            {
                // Callers pass today in various string shapes ("dd-MM-yyyy" from the
                // Hangfire jobs, "yyyyMMdd" from the real-time notifier). A plain
                // DateTime.Parse honors the SERVER culture: under en-US (typical on
                // hosting) "04-10-2026" becomes April 10, so the guard never matched
                // today's rows and every run re-sent. Parse exactly and culture-free;
                // every caller means "today", so fall back to it when unsure.
                string[] acceptedFormats = { "dd-MM-yyyy", "yyyyMMdd", "yyyy-MM-dd", "dd MMM yyyy" };
                if (!DateTime.TryParseExact(dateTime, acceptedFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime parsedDate))
                {
                    parsedDate = DateTime.Today;
                }
                var smsExists = await _context.PhoneSMS
                    .AnyAsync(s => s.MobileNumber == phoneNumber && s.SMSType == smsType && s.CreatedAt.Date == parsedDate.Date);
                return smsExists;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

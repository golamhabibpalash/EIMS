using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SMS.DAL.Contracts;
using SMS.DB;
using SMS.Entities;
using SMS.Entities.AdditionalModels.OnlineAdmission;
using SMS.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class AdmissionApplicationRepository : AdmissionRepository<AdmissionApplication>, IAdmissionApplicationRepository
    {
        public AdmissionApplicationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<AdmissionApplication> GetFullAsync(int id)
        {
            return _context.AdmissionApplications
                .Include(a => a.AdmissionSession).ThenInclude(s => s.AcademicSession)
                .Include(a => a.AdmissionProgram).ThenInclude(p => p.AcademicClass)
                .Include(a => a.AdmissionProgram).ThenInclude(p => p.AdmissionFeeHead)
                .Include(a => a.GuardianAccount)
                .Include(a => a.Gender)
                .Include(a => a.Religion)
                .Include(a => a.Nationality)
                .Include(a => a.BloodGroup)
                .Include(a => a.PresentDivision)
                .Include(a => a.PresentDistrict)
                .Include(a => a.PresentUpazila)
                .Include(a => a.PermanentDivision)
                .Include(a => a.PermanentDistrict)
                .Include(a => a.PermanentUpazila)
                .Include(a => a.Student)
                .Include(a => a.Documents).ThenInclude(d => d.AdmissionDocumentRequirement)
                .Include(a => a.Payments)
                .Include(a => a.History)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public Task<AdmissionApplication> GetTrackedWithChildrenAsync(int id)
        {
            return _context.AdmissionApplications
                .AsTracking()
                .Include(a => a.Documents)
                .Include(a => a.Payments)
                .AsSplitQuery()
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public Task<List<AdmissionApplication>> GetByGuardianAsync(int guardianAccountId)
        {
            return _context.AdmissionApplications
                .Include(a => a.AdmissionSession)
                .Include(a => a.AdmissionProgram).ThenInclude(p => p.AcademicClass)
                .Where(a => a.GuardianAccountId == guardianAccountId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public Task<List<AdmissionApplication>> SearchAsync(AdmissionApplicationFilter filter)
        {
            filter ??= new AdmissionApplicationFilter();

            var query = _context.AdmissionApplications
                .Include(a => a.AdmissionSession)
                .Include(a => a.AdmissionProgram).ThenInclude(p => p.AcademicClass)
                .Include(a => a.GuardianAccount)
                .AsQueryable();

            if (!filter.IncludeDrafts && filter.Status != AdmissionApplicationStatus.Draft)
            {
                query = query.Where(a => a.Status != AdmissionApplicationStatus.Draft);
            }
            if (filter.AdmissionSessionId.HasValue)
            {
                query = query.Where(a => a.AdmissionSessionId == filter.AdmissionSessionId.Value);
            }
            if (filter.AdmissionProgramId.HasValue)
            {
                query = query.Where(a => a.AdmissionProgramId == filter.AdmissionProgramId.Value);
            }
            if (filter.Status.HasValue)
            {
                query = query.Where(a => a.Status == filter.Status.Value);
            }
            if (filter.SubmittedFrom.HasValue)
            {
                var from = filter.SubmittedFrom.Value.Date;
                query = query.Where(a => a.SubmittedAt >= from);
            }
            if (filter.SubmittedTo.HasValue)
            {
                var to = filter.SubmittedTo.Value.Date.AddDays(1);
                query = query.Where(a => a.SubmittedAt < to);
            }
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim().ToLower();
                query = query.Where(a =>
                    (a.ApplicationNo != null && a.ApplicationNo.ToLower().Contains(term)) ||
                    (a.Name != null && a.Name.ToLower().Contains(term)) ||
                    (a.BirthCertificateNo != null && a.BirthCertificateNo.Contains(term)) ||
                    (a.GuardianPhone != null && a.GuardianPhone.Contains(term)) ||
                    a.GuardianAccount.MobileNumber.Contains(term));
            }

            return query
                .OrderByDescending(a => a.SubmittedAt ?? a.CreatedAt)
                .Take(filter.Take > 0 ? filter.Take : 500)
                .ToListAsync();
        }

        public Task<AdmissionApplication> FindByApplicationNoAsync(string applicationNo)
        {
            var no = (applicationNo ?? string.Empty).Trim().ToUpper();
            return _context.AdmissionApplications
                .Include(a => a.GuardianAccount)
                .Include(a => a.AdmissionProgram).ThenInclude(p => p.AcademicClass)
                .FirstOrDefaultAsync(a => a.ApplicationNo == no);
        }

        public Task<AdmissionApplication> FindActiveDuplicateAsync(int admissionSessionId, string birthCertificateNo, string name, DateTime? dob, int excludeApplicationId)
        {
            var closed = AdmissionStatusGroups.Closed.ToList();
            var certificate = (birthCertificateNo ?? string.Empty).Trim();
            var lowerName = (name ?? string.Empty).Trim().ToLower();
            var dobDate = dob?.Date;

            return _context.AdmissionApplications
                .Where(a => a.AdmissionSessionId == admissionSessionId
                         && a.Id != excludeApplicationId
                         && a.Status != AdmissionApplicationStatus.Draft
                         && !closed.Contains(a.Status))
                .Where(a => (certificate != "" && a.BirthCertificateNo == certificate)
                         || (lowerName != "" && dobDate != null && a.DOB == dobDate && a.Name.ToLower() == lowerName))
                .FirstOrDefaultAsync();
        }

        public Task<int> CountWithApplicationNoPrefixAsync(string prefix)
        {
            return _context.AdmissionApplications.CountAsync(a => a.ApplicationNo != null && a.ApplicationNo.StartsWith(prefix));
        }

        public Task<bool> ApplicationNoExistsAsync(string applicationNo)
        {
            return _context.AdmissionApplications.AnyAsync(a => a.ApplicationNo == applicationNo);
        }

        public Task<int> CountSeatsHeldAsync(int admissionProgramId)
        {
            var holding = AdmissionStatusGroups.HoldingSeat.ToList();
            return _context.AdmissionApplications.CountAsync(a => a.AdmissionProgramId == admissionProgramId && holding.Contains(a.Status));
        }

        public async Task<List<AdmissionSeatUsage>> GetSeatUsageAsync(int? admissionSessionId)
        {
            var query = _context.AdmissionApplications.AsQueryable();
            if (admissionSessionId.HasValue)
            {
                query = query.Where(a => a.AdmissionSessionId == admissionSessionId.Value);
            }

            var counts = await query
                .GroupBy(a => new { a.AdmissionProgramId, a.Status })
                .Select(g => new { g.Key.AdmissionProgramId, g.Key.Status, Count = g.Count() })
                .ToListAsync();

            return counts
                .GroupBy(c => c.AdmissionProgramId)
                .Select(g => new AdmissionSeatUsage
                {
                    AdmissionProgramId = g.Key,
                    SeatsHeld = g.Where(c => AdmissionStatusGroups.HoldingSeat.Contains(c.Status)).Sum(c => c.Count),
                    Enrolled = g.Where(c => c.Status == AdmissionApplicationStatus.Enrolled).Sum(c => c.Count),
                    Pending = g.Where(c => AdmissionStatusGroups.AwaitingDecision.Contains(c.Status)).Sum(c => c.Count),
                    Waitlisted = g.Where(c => c.Status == AdmissionApplicationStatus.Waitlisted).Sum(c => c.Count)
                })
                .ToList();
        }

        public async Task<Dictionary<AdmissionApplicationStatus, int>> GetStatusCountsAsync(int? admissionSessionId)
        {
            var query = _context.AdmissionApplications.AsQueryable();
            if (admissionSessionId.HasValue)
            {
                query = query.Where(a => a.AdmissionSessionId == admissionSessionId.Value);
            }

            var counts = await query
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(c => c.Status, c => c.Count);
        }
    }
}

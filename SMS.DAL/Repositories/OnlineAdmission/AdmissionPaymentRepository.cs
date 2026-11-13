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
    public class AdmissionPaymentRepository : AdmissionRepository<AdmissionPayment>, IAdmissionPaymentRepository
    {
        public AdmissionPaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task<AdmissionPayment> GetFullAsync(int id)
        {
            return _context.AdmissionPayments
                .Include(p => p.AdmissionApplication).ThenInclude(a => a.AdmissionSession)
                .Include(p => p.AdmissionApplication).ThenInclude(a => a.AdmissionProgram).ThenInclude(p => p.AcademicClass)
                .Include(p => p.AdmissionApplication).ThenInclude(a => a.GuardianAccount)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<List<AdmissionPayment>> SearchAsync(AdmissionPaymentStatus? status, int? admissionSessionId, int take = 500)
        {
            var query = _context.AdmissionPayments
                .Include(p => p.AdmissionApplication).ThenInclude(a => a.AdmissionProgram).ThenInclude(p => p.AcademicClass)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }
            if (admissionSessionId.HasValue)
            {
                query = query.Where(p => p.AdmissionApplication.AdmissionSessionId == admissionSessionId.Value);
            }

            return query.OrderByDescending(p => p.CreatedAt).Take(take).ToListAsync();
        }

        public Task<List<AdmissionPayment>> GetByApplicationAsync(int admissionApplicationId)
        {
            return _context.AdmissionPayments
                .Where(p => p.AdmissionApplicationId == admissionApplicationId)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }

        public Task<AdmissionPayment> FindByGatewayTransactionIdAsync(string gatewayTransactionId)
        {
            return _context.AdmissionPayments.FirstOrDefaultAsync(p => p.GatewayTransactionId == gatewayTransactionId);
        }
    }
}

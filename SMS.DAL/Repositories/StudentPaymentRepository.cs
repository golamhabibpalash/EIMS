using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.AdditionalModels.Finance;
using SMS.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories;

public class StudentPaymentRepository : Repository<StudentPayment>, IStudentPaymentRepository
{
    private readonly new ApplicationDbContext _context;

    public StudentPaymentRepository(ApplicationDbContext db) : base(db)
    {
        _context = db;
    }
    public override async Task<IReadOnlyCollection<StudentPayment>> GetAllAsync()
    {
        List<StudentPayment> payments = new List<StudentPayment>();
        try
        {
            payments = await _context
            .StudentPayment
            .Include(sp => sp.Student)
            .Include(sp => sp.StudentPaymentDetails)
            .ToListAsync();
        }
        catch (Exception)
        {

            throw;
        }

        return payments;
    }
    public async Task<IReadOnlyCollection<StudentPayment>> GetAllByStudentIdAsync(int id)
    {
        List<StudentPayment> payments = new List<StudentPayment>();
        try
        {
            payments = await _context.StudentPayment
            .Include(sp => sp.StudentPaymentDetails)
                .ThenInclude(sp => sp.StudentFeeHead)
            .Include(s => s.Student)
                .ThenInclude(ss => ss.AcademicClass)
            .Include(s => s.Student.AcademicSession)
            .Where(sp => sp.StudentId == id).ToListAsync();
        }
        catch (Exception)
        {

            throw;
        }
        return payments;
    }
    public async Task<List<StudentPaymentSummeryVM>> GetPaymentSummeryByDate(string date)
    {
        List<StudentPaymentSummeryVM> payments = new List<StudentPaymentSummeryVM>();
        try
        {
            if (DateTime.TryParse(date, out DateTime parsedDate))
            {
                payments = await _context.StudentPayment
                    .Include(sp => sp.Student).ThenInclude(s => s.AcademicClass)
                    .Where(sp => sp.PaidDate.Date == parsedDate.Date)
                    .GroupBy(sp => sp.Student.AcademicClass.Name)
                    .Select(g => new StudentPaymentSummeryVM
                    {
                        AcademicClassName = g.Key,
                        Payments = g.Sum(sp => sp.TotalPayment)
                    })
                    .ToListAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return payments;
    }
    public async Task<List<StudentPaymentSummerySMS_VM>> GetStudentPaymentSummerySMS_VMsAsync(DateTime date)
    {
        List<StudentPaymentSummerySMS_VM> payments = new List<StudentPaymentSummerySMS_VM>();
        try
        {
            var datePayments = await _context.StudentPayment
                .Include(sp => sp.Student)
                .Where(sp => sp.PaidDate.Date == date.Date)
                .ToListAsync();

            decimal residential = (decimal)datePayments.Where(sp => sp.Student.IsResidential).Sum(sp => sp.TotalPayment);
            decimal nonResidential = (decimal)datePayments.Where(sp => !sp.Student.IsResidential).Sum(sp => sp.TotalPayment);

            payments.Add(new StudentPaymentSummerySMS_VM
            {
                ResidentialPayment = residential,
                NonResidentialPayment = nonResidential
            });
        }
        catch (Exception)
        {
            throw;
        }

        return payments;
    }
    public async Task<List<StudentPaymentSummeryVM>> GetPaymentSummeryByMonthYear(string monthYear)
    {
        List<StudentPaymentSummeryVM> payments = new List<StudentPaymentSummeryVM>();
        try
        {
            if (monthYear.Length == 7 && int.TryParse(monthYear[..4], out int year) && int.TryParse(monthYear[5..7], out int month))
            {
                payments = await _context.StudentPayment
                    .Include(sp => sp.Student).ThenInclude(s => s.AcademicClass)
                    .Where(sp => sp.PaidDate.Year == year && sp.PaidDate.Month == month)
                    .GroupBy(sp => sp.Student.AcademicClass.Name)
                    .Select(g => new StudentPaymentSummeryVM
                    {
                        AcademicClassName = g.Key,
                        Payments = g.Sum(sp => sp.TotalPayment)
                    })
                    .ToListAsync();
            }
            else if (monthYear.Length == 6 && int.TryParse(monthYear[..4], out int y) && int.TryParse(monthYear[4..6], out int m))
            {
                payments = await _context.StudentPayment
                    .Include(sp => sp.Student).ThenInclude(s => s.AcademicClass)
                    .Where(sp => sp.PaidDate.Year == y && sp.PaidDate.Month == m)
                    .GroupBy(sp => sp.Student.AcademicClass.Name)
                    .Select(g => new StudentPaymentSummeryVM
                    {
                        AcademicClassName = g.Key,
                        Payments = g.Sum(sp => sp.TotalPayment)
                    })
                    .ToListAsync();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return payments;
    }
    public override async Task<StudentPayment> GetByIdAsync(int id)
    {
        StudentPayment existingStudentPayment = await _context.StudentPayment
            .Include(s => s.Student)
                .ThenInclude(s => s.AcademicClass)
            .Include(s => s.Student.AcademicSession)
            .Include(s => s.StudentPaymentDetails)
                .ThenInclude(d => d.StudentFeeHead)
            .Where(s => s.Id == id).FirstOrDefaultAsync();

        return existingStudentPayment;
    }
    public async Task<List<StudentPaymentScheduleVM>> GetStudentPaymentSchedule(int studId)
    {
        List<StudentPaymentScheduleVM> finalPaymentScheduleVMs = new List<StudentPaymentScheduleVM>();
        try
        {
            var student = await _context.Student.FirstOrDefaultAsync(s => s.Id == studId);
            if (student == null) return finalPaymentScheduleVMs;

            var admissionMonth = student.AdmissionDate.Date.Month;

            var classFeeList = await _context.ClassFeeList
                .Include(c => c.StudentFeeHead)
                .Where(c => c.AcademicClassId == student.AcademicClassId && c.AcademicSessionId == student.AcademicSessionId)
                .OrderBy(c => c.SL)
                .ToListAsync();

            var existingFeeAllocations = await _context.StudentFeeAllocations
                .Where(s => s.UniqueId == student.UniqueId && s.IsActive)
                .ToListAsync();

            var admissionOrSession = student.AdmissionDate.Year < DateTime.Now.Year ? 13 : 0;

            foreach (var item in classFeeList)
            {
                if (item.SL >= 1 && item.SL <= 12)
                {
                    if (item.SL < admissionMonth)
                    {
                        continue;
                    }
                }

                if ((admissionOrSession == 13 && item.SL == 0) || (admissionOrSession == 0 && item.SL == 13))
                {
                    continue;
                }

                var feeAllocation = existingFeeAllocations
                    .FirstOrDefault(s => s.StudentFeeHeadId == item.StudentFeeHeadId && s.ClassFeeListId == item.Id);

                finalPaymentScheduleVMs.Add(new StudentPaymentScheduleVM
                {
                    PaymentType = item.StudentFeeHead.Name,
                    Amount = feeAllocation != null ? feeAllocation.AllocatedAmount : item.Amount,
                    yearlyFrequency = item.StudentFeeHead.YearlyFrequency ?? 0,
                    IsResidential = item.StudentFeeHead.IsResidential,
                    SL = item.SL ?? 0,
                    FeeHeadId = item.StudentFeeHeadId,
                    ClassFeeId = item.Id
                });
            }
        }
        catch (Exception)
        {
            throw;
        }

        return finalPaymentScheduleVMs;
    }
    public async Task<List<StudentPaymentSchedulePaidVM>> GetStudentPaymentSchedulePaid(int studId)
    {
        List<StudentPaymentSchedulePaidVM> studentPaymentSchedules = new List<StudentPaymentSchedulePaidVM>();
        try
        {
            studentPaymentSchedules = await _context.StudentPayment
                .Include(sp => sp.StudentPaymentDetails).ThenInclude(pd => pd.StudentFeeHead)
                .Where(sp => sp.StudentId == studId)
                .SelectMany(sp => sp.StudentPaymentDetails)
                .GroupBy(pd => new { pd.StudentFeeHeadId, pd.StudentFeeHead.Name })
                .Select(g => new StudentPaymentSchedulePaidVM
                {
                    PaymentType = g.Key.Name,
                    PaymentCount = g.Count(),
                    PaidAmount = g.Sum(pd => pd.PaidAmount),
                    FeeHeadSL = g.Key.StudentFeeHeadId
                })
                .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
        return studentPaymentSchedules;
    }

    public async Task<IEnumerable<StudentPayment>> GetAllByStudentUniqueIdAsync(string uniqueId)
    {
        List<StudentPayment> payments = new List<StudentPayment>();
        try
        {
            // Payment rows were historically written with inconsistent zero-padding
            // ("02607009" vs "2607009"), so an exact match silently drops history.
            var keys = AttendancePinMatcher.KeyVariants(uniqueId);
            payments = await _context.StudentPayment
            .Include(sp => sp.StudentPaymentDetails)
                .ThenInclude(sp => sp.StudentFeeHead)
            .Include(s => s.Student)
                .ThenInclude(ss => ss.AcademicClass)
            .Include(s => s.Student.AcademicSession)
            .Where(sp => sp.UniqueId != null && keys.Contains(sp.UniqueId.Trim())).ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
        return payments;
    }
    public async Task<List<PaidAmountResult>> GetPaidAmountByFeeHead(string uniqueId, int sessionId, int isResidential, int classId, int feeHeadId)
    {
        List<PaidAmountResult> result = new List<PaidAmountResult>();
        try
        {
            var keys = AttendancePinMatcher.KeyVariants(uniqueId);
            var paidAmount = await _context.StudentPaymentDetails
                .Include(pd => pd.StudentPayment)
                .Where(pd => pd.StudentPayment.UniqueId != null && keys.Contains(pd.StudentPayment.UniqueId.Trim())
                    && pd.StudentPayment.AcademicSessionId == sessionId
                    && pd.StudentFeeHeadId == feeHeadId)
                .SumAsync(pd => pd.PaidAmount);

            result.Add(new PaidAmountResult { PaidAmount = (decimal)paidAmount });
        }
        catch (Exception)
        {
            throw;
        }
        return result;
    }

    public async Task<List<PreviouisPaymentDetailsDto>> GetAllStudentsPaymentSummeryAsync()
    {
        try
        {
            var students = await _context.Student
                .Include(s => s.AcademicClass)
                .Include(s => s.AcademicSection)
                .Include(s => s.AcademicSession)
                .Where(s => s.Status)
                .ToListAsync();

            var uniqueIds = students.Select(s => s.UniqueId).ToList();

            var allocations = await _context.StudentFeeAllocations
                .Where(a => a.IsActive && a.UniqueId != null && uniqueIds.Contains(a.UniqueId))
                .GroupBy(a => a.UniqueId)
                .Select(g => new { UniqueId = g.Key, TotalPayable = g.Sum(a => a.AllocatedAmount) })
                .ToListAsync();

            var payments = await _context.StudentPayment
                .Where(p => uniqueIds.Contains(p.UniqueId))
                .GroupBy(p => p.UniqueId)
                .Select(g => new { UniqueId = g.Key, TotalPaid = g.Sum(p => p.TotalPayment) })
                .ToListAsync();

            var result = students.Select(s =>
            {
                var alloc = allocations.FirstOrDefault(a => a.UniqueId == s.UniqueId);
                var pay = payments.FirstOrDefault(p => p.UniqueId == s.UniqueId);
                var payable = alloc?.TotalPayable ?? 0;
                var paid = pay?.TotalPaid ?? 0;

                return new PreviouisPaymentDetailsDto
                {
                    UniqueId = s.UniqueId,
                    StudentId = s.Id,
                    AcademicSectionId = s.AcademicSectionId,
                    CurrentClassId = s.AcademicClassId,
                    PayableAmount = payable,
                    PaidAmount = paid,
                    DueAmount = payable - paid,
                    Status = s.Status
                };
            }).ToList();

            return result;
        }
        catch (Exception)
        {
            return new List<PreviouisPaymentDetailsDto>();
        }
    }

    public async Task<DuePaymentBulkResult> GetBulkDuePaymentsAsync(int sessionId, List<int> studentIds)
    {
        var result = new DuePaymentBulkResult();

        var uniqueIds = await _context.Student
            .Where(s => studentIds.Contains(s.Id))
            .Select(s => s.UniqueId)
            .ToListAsync();

        result.CurrentSession = await _context.AcademicSession.FirstOrDefaultAsync(s => s.Id == sessionId);

        var classIds = await _context.Student
            .Where(s => studentIds.Contains(s.Id))
            .Select(s => s.AcademicClassId)
            .Distinct()
            .ToListAsync();

        result.ClassFees = await _context.ClassFeeList
            .Include(c => c.StudentFeeHead)
            .Include(c => c.AcademicSession)
            .Where(c => c.AcademicSessionId == sessionId && classIds.Contains(c.AcademicClassId))
            .ToListAsync();

        var classFeeIds = result.ClassFees.Select(c => c.Id).ToList();

        result.Allocations = await _context.StudentFeeAllocations
            .Include(a => a.StudentFeeHead)
            .Where(a => a.UniqueId != null && uniqueIds.Contains(a.UniqueId) && a.IsActive && a.ClassFeeListId.HasValue && classFeeIds.Contains(a.ClassFeeListId.Value))
            .ToListAsync();

        result.Payments = await _context.StudentPayment
            .Include(p => p.StudentPaymentDetails)
            .Where(p => uniqueIds.Contains(p.UniqueId) && p.AcademicSessionId == sessionId)
            .ToListAsync();

        return result;
    }

}
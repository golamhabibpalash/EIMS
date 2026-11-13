using SMS.BLL.Contracts.Reports;
using SMS.DAL.Contracts.Reports;
using SMS.Entities;
using SMS.Entities.RptModels;
using SMS.Entities.RptModels.AttendanceVM;
using SMS.Entities.RptModels.Results;
using SMS.Entities.RptModels.StudentPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Managers.Reports
{
    public class ReportManager : IReportManager
    {
        private readonly IReportRepository _reportRepository;
        public ReportManager(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<RptAdmitCardVM>> GetAdmitCard(int monthId, int academicClassId, int academicSectionId, int examTypeId, int examGroupId = 0)
        {
            var admitCards = await _reportRepository.GetAdmitCard(monthId, academicClassId, academicSectionId, examTypeId, examGroupId);

            List<RptAdmitCardVM> filteredAdmitCards = new List<RptAdmitCardVM>();
            foreach (var admitCard in admitCards)
            {
                var isExist = filteredAdmitCards.Any(s => s.SubjectCode == admitCard.SubjectCode && s.ClassRoll == admitCard.ClassRoll);
                if (isExist)
                {
                    continue;
                }
                if (admitCard.StudentStauts)
                {
                    filteredAdmitCards.Add(admitCard);
                }
            }

            return filteredAdmitCards.OrderBy(s => s.ClassRoll).ToList();
        }

        public async Task<List<RptDailyAttendaceVM>> GetDailyAttendanceReport(string fromDate, string academicClassId, string academicSectionId,string attendanceType, string aSessionId, string attendanceFor)
        {
            return await _reportRepository.GetDailyAttendanceReport(fromDate, academicClassId, academicSectionId, attendanceType, aSessionId, attendanceFor);
        }

        public async Task<List<RptDailyAttendaceVM>> GetDailyAttendanceReportCheckOut(string fromDate, string academicClassId, string academicSectionId, string attendanceType, string aSessionId, string attendanceFor)
        {
            return await _reportRepository.GetDailyAttendanceReportCheckOut(fromDate, academicClassId, academicSectionId, attendanceType, aSessionId, attendanceFor);
        }

        public async Task<List<RptStudentsPaymentVM>> GetStudentPayment(string fromDate, string ToDate, string AcademicClassId, string AcademicSectionId)
        {
            return await _reportRepository.GetStudentPayment(fromDate,ToDate,AcademicClassId, AcademicSectionId);
        }

        public async Task<List<rptStudentPaymentsVM>> GetStudentPaymentsByRoll(int classRoll, string fromDate, string toDate)
        {
            if (string.IsNullOrEmpty(fromDate))
            {
                fromDate = new DateTime(DateTime.Today.Year, 1, 1).ToString();
            }
            if (string.IsNullOrEmpty(toDate))
            {
                toDate = DateTime.Today.Date.ToString();
            }
            return await _reportRepository.GetStudentPaymentsByRoll(classRoll,fromDate,toDate);
        }

        public async Task<List<RptStudentVM>> GetStudentsInfo(int AcademicSessionId, int? AcademicClassId, int? AcademicSectionId)
        {
            return await _reportRepository.getStudentsInfo(AcademicSessionId, AcademicClassId, AcademicSectionId);
        }
        public async Task<List<RptPaymentReceiptVM>> GetPaymentReceiptReport(int paymentId)
        {
            return await _reportRepository.GetPaymentReceiptReport(paymentId);
        }

        public async Task<List<SubjectWiseMarkSheetVM>> GetSubjectWiseMarkSheet(int examId)
        {
            return await _reportRepository.GetSubjectWiseMarkSheet(examId);
        }

        public async Task<List<StudentWiseMarkSheetVM>> GetStudentWiseMarkSheet(int examGroupId, int classId)
        {
            var result = await _reportRepository.GetStudentWiseMarkSheet(examGroupId, classId);
            return result;
        }

    }
}

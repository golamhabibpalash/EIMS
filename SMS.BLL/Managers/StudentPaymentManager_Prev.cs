//using BLL.Managers.Base;
//using SMS.BLL.Contracts;
//using SMS.DAL.Contracts;
//using SMS.Entities;
//using SMS.Entities.AdditionalModels;
//using SMS.Entities.Enums;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace SMS.BLL.Managers;

//public class StudentPaymentManager : Manager<StudentPayment>, IStudentPaymentManager
//{
//    private readonly IStudentPaymentRepository _studentPaymentRepository;
//    private readonly IStudentRepository _studentRepository;
//    private readonly IClassFeeListRepository _classFeeListRepository;
//    private readonly IAcademicSessionRepository _academicSessionRepository;
//    private readonly IStudentFeeHeadRepository _studentFeeHeadRepository;
//    private readonly IStudentPaymentDetailsRepository _studentPaymentDetailsRepository;
//    private readonly IStudentFeeAllocationRepository _studentFeeAllocationRepository;
//    private readonly IParamBusConfigManager _paramBusConfigManager;

//    public StudentPaymentManager(IStudentPaymentRepository studentPaymentRepository, IStudentRepository studentRepository, IClassFeeListRepository classFeeListRepository, IAcademicSessionRepository academicSessionRepository, IStudentFeeHeadRepository studentFeeHeadRepository, IStudentPaymentDetailsRepository studentPaymentDetailsRepository, IStudentFeeAllocationRepository studentFeeAllocationRepository, IParamBusConfigManager paramBusConfigManager) : base(studentPaymentRepository)
//    {
//        _studentPaymentRepository = studentPaymentRepository;
//        _studentRepository = studentRepository;
//        _classFeeListRepository = classFeeListRepository;
//        _academicSessionRepository = academicSessionRepository;
//        _studentFeeHeadRepository = studentFeeHeadRepository;
//        _studentPaymentDetailsRepository = studentPaymentDetailsRepository;
//        _studentFeeAllocationRepository = studentFeeAllocationRepository;
//        _paramBusConfigManager = paramBusConfigManager;
//    }

//    public async Task<IReadOnlyCollection<StudentPayment>> GetAllByStudentIdAsync(int id)
//    {
//        return await _studentPaymentRepository.GetAllByStudentIdAsync(id);
//    }

//    public async Task<string> GetNewReceipt(int studentId, int feeHeadId)
//    {
//        string receiptsNo = string.Empty;
//        Student student = await _studentRepository.GetByIdAsync(studentId);
//        var allPayments = await _studentPaymentRepository.GetAllAsync();
//        var sl = ((from p in allPayments
//                   where p.PaidDate.ToString("yyMM") == DateTime.Today.ToString("yyMM")
//                   select p).Count() + 1).ToString().PadLeft(3, '0');
//        receiptsNo = student.AcademicClassId.ToString() + feeHeadId.ToString() + DateTime.Now.ToString("yyMM") + sl;

//        return receiptsNo;
//    }

//    public async Task<IReadOnlyCollection<StudentPaymentSummeryVM>> GetPaymentSummeryByDate(string date)
//    {
//        List<StudentPaymentSummeryVM> paymentSummery = new List<StudentPaymentSummeryVM>();
//        try
//        {
//            paymentSummery = await _studentPaymentRepository.GetPaymentSummeryByDate(date);
//        }
//        catch (Exception)
//        {

//            throw;
//        }
//        return paymentSummery;
//    }

//    public async Task<IReadOnlyCollection<StudentPaymentSummeryVM>> GetPaymentSummeryByMonthYear(string monthYear)
//    {
//        List<StudentPaymentSummeryVM> paymentSummery = new List<StudentPaymentSummeryVM>();
//        try
//        {
//            paymentSummery = await _studentPaymentRepository.GetPaymentSummeryByMonthYear(monthYear);
//        }
//        catch (Exception)
//        {

//            throw;
//        }
//        return paymentSummery;
//    }

//    public async Task<List<StudentPaymentScheduleVM>> GetStudentPaymentSchedule(int studId)
//    {
//        return await _studentPaymentRepository.GetStudentPaymentSchedule(studId);
//    }

//    public async Task<List<StudentPaymentSchedulePaidVM>> GetStudentPaymentSchedulePaid(int studId)
//    {
//        return await _studentPaymentRepository.GetStudentPaymentSchedulePaid(studId);
//    }

//    public async Task<List<StudentPaymentSummerySMS_VM>> GetStudentPaymentSummerySMS_VMsAsync(DateTime date)
//    {
//        List<StudentPaymentSummerySMS_VM> paymentSummery = new List<StudentPaymentSummerySMS_VM>();
//        try
//        {
//            paymentSummery = await _studentPaymentRepository.GetStudentPaymentSummerySMS_VMsAsync(date);
//        }
//        catch (Exception)
//        {

//            throw;
//        }
//        return paymentSummery;
//    }
//    public async Task<double> GetStudentCurrentDue(int stuId)
//    {
//        var student = await _studentRepository.GetByIdAsync(stuId);
//        if (student == null) return 0;

//        var session = await _academicSessionRepository.GetCurrentAcademicSession();
//        if (session == null) return 0;

//        var classFees = await GetFilteredClassFees(student);
//        var allocations = await GetFilteredAllocations(student);

//        double admissionOrSessionFee = CalculateAdmissionOrSessionFee(student, session, classFees, allocations);
//        double monthlyFee = CalculateMonthlyFee(student, classFees, allocations);
//        double otherFee = CalculateOtherFee(student, classFees, allocations);

//        double totalScheduledFee = admissionOrSessionFee + monthlyFee + otherFee;
//        double totalPaid = await GetStudentTotalPaidAmountBySession(student.AcademicSessionId, student.UniqueId);

//        return totalScheduledFee - totalPaid;
//    }
//    private async Task<List<ClassFeeList>> GetFilteredClassFees(Student student)
//    {
//        var allClassFee = await _classFeeListRepository.GetByClassIdSessionIdStudentIdAsync(student.AcademicClassId, student.AcademicSessionId, student.Id);        

//        return allClassFee;
//    }

//    private async Task<List<StudentFeeAllocation>> GetFilteredAllocations(Student student)
//    {
//        return (await _studentFeeAllocationRepository
//            .GetStudentFeeAllocationByUniqueIdSessionId(student.UniqueId, student.AcademicSessionId))
//            .Where(a => a.IsActive)
//            .ToList();
//    }

//    private double CalculateAdmissionOrSessionFee(Student student, AcademicSession session, List<ClassFeeList> classFees, List<StudentFeeAllocation> allocations)
//    {
//        bool isAdmission = student.AdmissionDate.Year.ToString() == session.Name[^4..];
//        int feeSl = isAdmission ? 0 : 13;

//        var classFee = classFees.FirstOrDefault(f => f.StudentFeeHead.SL == feeSl);
//        var allocation = allocations.FirstOrDefault(a => a.ClassFeeListId == classFee?.Id);

//        return GetAdmissionOrSessionScheduleFee(student.AcademicSessionId, student.AcademicClassId, student.UniqueId, classFee, allocation);
//    }

//    private double CalculateMonthlyFee(Student student, List<ClassFeeList> classFees, List<StudentFeeAllocation> allocations)
//    {
//        int admissionYear = student.AdmissionDate.Year;
//        int admissionMonth = student.AdmissionDate.Month;

//        var monthlyFees = classFees
//            .Where(cFee =>
//                cFee.Amount > 0 &&
//                cFee.StudentFeeHead.SL is >= 1 and <= 12 &&
//                ShouldIncludeFeeForMonth(cFee, admissionYear, admissionMonth))
//            .ToList();

//        var monthlyAllocations = allocations
//            .Where(a => a.StudentFeeHead.SL is >= 1 and <= 12)
//            .ToList();

//        return GetMonthlyScheduledFee(
//            student.AcademicClassId,
//            student.AcademicSessionId,
//            student.UniqueId,
//            monthlyFees,
//            monthlyAllocations
//        );
//    }
//    private bool ShouldIncludeFeeForMonth(ClassFeeList cFee, int admissionYear, int admissionMonth)
//    {
//        if (int.TryParse(cFee.AcademicSession.Name[^4..], out int sessionYear))
//        {
//            bool isAdmissionYear = sessionYear == admissionYear;

//            // Exclude months before admission in the admission year
//            if (isAdmissionYear && cFee.SL < admissionMonth)
//            {
//                return false;
//            }
//        }
//        return true;
//    }

//    private double CalculateOtherFee(Student student, List<ClassFeeList> classFees, List<StudentFeeAllocation> allocations)
//    {
//        var otherFees = classFees
//            .Where(f => f.StudentFeeHead.SL > 13)
//            .ToList();

//        var otherAllocations = allocations
//            .Where(a => a.StudentFeeHead.SL > 13)
//            .ToList();

//        return GetOtherScheduledFee(student.AcademicClassId, student.AcademicSessionId, student.UniqueId, otherFees, otherAllocations);
//    }

//    private double GetAdmissionOrSessionScheduleFee(int sessionId, int classId, string uniqueId, ClassFeeList AdmissionOrSessionClassFee, StudentFeeAllocation allocation)
//    {
//        //admission/session fee
//        // SL 0 = admission fee & 13 = session fee
//        double admissionOrSessionFee = 0.0;
        
//         //0 is for admission and 13 is for session

//        if (AdmissionOrSessionClassFee != null)
//        {
            
//            admissionOrSessionFee = allocation?.AllocatedAmount ?? AdmissionOrSessionClassFee.Amount;
//        }
//        return admissionOrSessionFee;
//    }
//    private double GetMonthlyScheduledFee(int classId, int sessionId, string uniqueId, List<ClassFeeList> monthlyClassFees, List<StudentFeeAllocation> allAllocations)
//    {
//        var monthlyPaymentFee = 0.0;
//        var currentMonth = DateTime.Now.Month;
//        foreach (var classFee in monthlyClassFees.OrderBy(s => s.StudentFeeHead.SL))
//        {
//            var expectedAllocation  = allAllocations.FirstOrDefault(s => s.ClassFeeListId == classFee.Id);
//            monthlyPaymentFee += expectedAllocation != null ? expectedAllocation.AllocatedAmount : classFee.Amount;
//            if (classFee.StudentFeeHead.SL > currentMonth)
//            {
//                break;
//            }
//        }

//        return monthlyPaymentFee;
//    }
//    private double GetOtherScheduledFee(int classId, int sessionId, string uniqueId, List<ClassFeeList> othersClassFees, List<StudentFeeAllocation> allAllocations)
//    {
//        var otherScheduledFee = 0.0;
//        var currentMonth = DateTime.Now.Month;
//        foreach (var classFee in othersClassFees)
//        {
//            var expectedAllocation = allAllocations.FirstOrDefault(s => s.ClassFeeListId == classFee.Id);
//            otherScheduledFee += expectedAllocation != null ? expectedAllocation.AllocatedAmount : classFee.Amount;
//            if (classFee.StudentFeeHead.SL > currentMonth)
//            {
//                break;
//            }
//        }

//        return otherScheduledFee;
//    }
//    public async Task<List<StudentPayment>> GetPaymentByStudentUniqueId(string uniqueId)
//    {
//        List<StudentPayment> sps = new List<StudentPayment>();
//        var paymens = await _studentPaymentRepository.GetAllAsync();
//        sps = paymens.Where(p => p.UniqueId == uniqueId).ToList();
//        return sps;
//    }

//    public async Task<List<PaidAmountResult>> GetPaidAmountByFeeHeadAsync(string uniqueId, int sessionId, int isResidential, int classId, int feeHeadId)
//    {
//        var result = await _studentPaymentRepository.GetPaidAmountByFeeHead(uniqueId, sessionId, isResidential, classId, feeHeadId);
//        return result;
//    }

//    public async Task<StudentPaymentDetailVM> GetAllDetailPaymentByUniqueId(string studentUniqueId)
//    {
//        var student = await _studentRepository.GetStudentByUniqueIdAsync(studentUniqueId.Trim());
//        var currentSession = await _academicSessionRepository.GetCurrentAcademicSession();
//        if (student != null)
//        {
//            var studentPayments = await _studentPaymentRepository.GetAllByStudentUniqueIdAsync(studentUniqueId);
//            StudentPaymentDetailVM studentPaymentDetailVM = new StudentPaymentDetailVM();
//            var allSessions = await _academicSessionRepository.GetAllAsync();
//            foreach (var session in allSessions.OrderByDescending(s => s.Name))
//            {
//                var isPayment = studentPayments.Any(s => s.AcademicSessionId == session.Id);
//                if (isPayment)
//                {
//                    var sessionWisePaymentDetails = studentPayments.Where(s => s.AcademicSessionId == session.Id).Select(s => s.StudentPaymentDetails).ToList();
//                    var classFeeId = sessionWisePaymentDetails.FirstOrDefault().Select(s => s.ClassFeeId).FirstOrDefault();
//                    var classFee = await _classFeeListRepository.GetByIdAsync(classFeeId);

//                    var tAmount = await GetStudentTotalPaybleAmountBySessionAsync(session.Id, student.UniqueId, classFee.AcademicClassId);
//                    var pAmount = await GetStudentTotalPaidAmountBySession(session.Id, student.UniqueId);
//                    SinglePaymentVM singlePaymentVM = new SinglePaymentVM()
//                    {
//                        PaymentsTitle = "Detail Payments ",
//                        AcademicSession = session.Name,
//                        CurrentSession = currentSession.Name,
//                        TotalAmount = tAmount,
//                        TotalPaidAmount = pAmount,
//                        TotalDueAmount = await GetStudentCurrentDue(student.Id),
//                        SessionWisePaymentVMs = await GetSessionWisePaymentVMs(session.Id, studentUniqueId, classFee.AcademicClassId)
//                    };
//                    studentPaymentDetailVM.Payments.Add(singlePaymentVM);

//                }
//            }
//            return studentPaymentDetailVM;
//        }
//        return null;
//    }
//    private async Task<List<SessionWisePaymentVM>> GetSessionWisePaymentVMs(int sessionId, string uniqueId, int classId)
//    {
//        var allParamConfig = await _paramBusConfigManager.GetAllAsync();
//        var student = await _studentRepository.GetStudentByUniqueIdAsync(uniqueId);
//        string sessionFeeName = student.IsResidential? allParamConfig.FirstOrDefault(s => s.ParamSL==17).ParamValue : allParamConfig.FirstOrDefault(s => s.ParamSL == 16).ParamValue;
//        string admissionFeeName = student.IsResidential ? allParamConfig.FirstOrDefault(s => s.ParamSL == 15).ParamValue : allParamConfig.FirstOrDefault(s => s.ParamSL == 14).ParamValue;
//        List<SessionWisePaymentVM> sessionWisePaymentVMs = new List<SessionWisePaymentVM>();
//        var allClassFees = await _classFeeListRepository.GetAllBySessionIdClassIdAsync(sessionId, classId);
//        allClassFees = allClassFees.Where(s => s.StudentFeeHead.IsResidential == student.IsResidential).ToList();

//        var allPaymentDetailsByStudent = await _studentPaymentDetailsRepository.GetAllByStudentAsync(uniqueId);
//        var admissionYear = student.AdmissionDate.Year.ToString();
//        var nowSession = await _academicSessionRepository.GetByIdAsync(sessionId);
//        var sessionYear = nowSession.Name.Substring(nowSession.Name.Length - 4, 4);
//        var allAllocationsBySessionId = await _studentFeeAllocationRepository.GetStudentFeeAllocationByUniqueIdSessionId(uniqueId, sessionId);
//        if (admissionYear == sessionYear)
//        {

//        }
//        else
//        {

//        }
//        if (allClassFees != null)
//        {
//            foreach (var classFee in allClassFees.OrderBy(s => s.SL))
//            {
//                //skip if fee is zero
//                if (classFee.Amount==0)
//                {
//                    continue;
//                }

//                var classFeeList = await _classFeeListRepository.GetClassFeeListByClassIdFeeHeadIdSessionIdAsync(classId, classFee.StudentFeeHeadId, sessionId);

//                //Admin or Session
//                var isAdmittedByThisSession = allPaymentDetailsByStudent.Any(s => s.StudentFeeHeadId == 2 || s.StudentFeeHeadId == 6);

//                if (isAdmittedByThisSession)
//                {
//                    if (classFee.StudentFeeHead.Name == sessionFeeName)
//                    {
//                        continue;
//                    }
//                }
//                else
//                {
//                    if (classFee.StudentFeeHead.Name == admissionFeeName)
//                    {
//                        continue;
//                    }
//                }
//                //admission date wise filter
//                var sessionName = classFee.AcademicSession.Name;
//                if (sessionName.Substring(sessionName.Length-4) == student.AdmissionDate.Year.ToString())
//                {
//                    if (classFee.SL is >= 1 and <= 12 && classFee.SL < student.AdmissionDate.Month)
//                    {
//                        continue;
//                    }
//                }

//                var totalAmount = classFeeList.Select(s => s.Amount).FirstOrDefault();

//                var allocations = allAllocationsBySessionId.FirstOrDefault(s => s.ClassFeeListId == classFee.Id);
//                if (allocations != null)
//                {
//                    totalAmount = allocations.AllocatedAmount;
//                }
//                var paidAmount = allPaymentDetailsByStudent.Where(d => d.ClassFeeId == classFeeList.Select(c => c.Id).FirstOrDefault() && d.StudentFeeHeadId == classFee.StudentFeeHeadId).Select(s => s.PaidAmount).Sum();

//                SessionWisePaymentVM sessionWisePaymentVM = new SessionWisePaymentVM()
//                {
//                    FeeHeadName = classFee.StudentFeeHead.Name,
//                    Amount = totalAmount,
//                    PaidAmount = paidAmount,
//                    Balance = totalAmount - paidAmount,
//                    Status = GetPaymentStatus(totalAmount, paidAmount),
//                    SessionWisePaymentDetails = await GetSessionWisePaymentDetails(classFee.StudentFeeHeadId, uniqueId, sessionId, totalAmount)
//                };
//                sessionWisePaymentVMs.Add(sessionWisePaymentVM);
//            }
//            return sessionWisePaymentVMs;
//        }
//        return null;
//    }

//    private async Task<List<SessionWisePaymentDetails>> GetSessionWisePaymentDetails(int feeHeadId, string uniqueId, int sessionId, double totalAmount)
//    {
//        var sPaymentDetails = await _studentPaymentDetailsRepository.GetAllByStudentAsync(uniqueId);
//        sPaymentDetails = sPaymentDetails.Where(p => p.StudentPayment.AcademicSessionId == sessionId && p.StudentFeeHeadId == feeHeadId).ToList();
//        var paymentDetails = new List<SessionWisePaymentDetails>();
//        if (sPaymentDetails != null)
//        {
//            var totalPaid = 0.0;
//            var paybleAmont = 0.0;
//            foreach (var item in sPaymentDetails)
//            {
//                paybleAmont = totalAmount - totalPaid;
//                totalPaid += item.PaidAmount;
//                var pAmount = item.PaidAmount;
//                var restAmount = totalAmount - totalPaid;
//                SessionWisePaymentDetails sessionWisePaymentDetails = new SessionWisePaymentDetails()
//                {
//                    PaymentDetailId = item.Id,
//                    PaidDate = item.CreatedAt.ToString("dd MMM yyyy"),
//                    ReceiptNo = item.StudentPayment.ReceiptNo,
//                    PayableAmount = paybleAmont,
//                    PaidAmount = item.PaidAmount,
//                    DueAmount = restAmount,
//                    Remarks = item.StudentPayment.Remarks,
//                    Status = GetPaymentStatus(paybleAmont, pAmount),
//                    PaymentId = item.StudentPayment.Id
//                };
//                paymentDetails.Add(sessionWisePaymentDetails);
//            }
//            ;
//        }
//        return paymentDetails;
//    }

//    private string GetPaymentStatus(double amount, double paidAmount)
//    {
//        var status = string.Empty;
//        if (paidAmount == 0 && (amount > paidAmount))
//        {
//            status = StudentPaymentStatus.Unpaid.ToString();
//        }
//        else if (amount > paidAmount)
//        {
//            status = StudentPaymentStatus.Partial.ToString();
//        }
//        else if (amount == paidAmount || amount < paidAmount)
//        {
//            status = StudentPaymentStatus.Paid.ToString();
//        }
//        return status;
//    }

//    public async Task<double> GetStudentTotalPaybleAmountBySessionAsync(int sessionId, string studentUniqueId, int classId)
//    {
//        var totalFees = 0.00;
//        var student = await _studentRepository.GetStudentByUniqueIdAsync(studentUniqueId);
//        var allClassFeeBySessionId = await _classFeeListRepository
//            .GetAllBySessionIdClassIdAsync(sessionId, classId, student.IsResidential);

//        List<ClassFeeList> classFeeLists = new List<ClassFeeList>();


//        //Admin or Session
//        DateTime addmissionYear = student.AdmissionDate;
//        AcademicSession academicSession = await _academicSessionRepository.GetByIdAsync(sessionId);

//        var studentAllAllocations = await _studentFeeAllocationRepository.GetStudentFeeAllocationByUniqueIdSessionId(studentUniqueId, sessionId);
//        if (allClassFeeBySessionId != null)
//        {
//            foreach (var cFee in allClassFeeBySessionId)
//            {
//                //filter if amount is zero
//                if (cFee.Amount==0)
//                {
//                    continue;
//                }

//                // Extract common values once
//                int admissionYear = student.AdmissionDate.Year;
//                int admissionMonth = student.AdmissionDate.Month;

//                //filter by admission date
//                string sessionYearText = cFee.AcademicSession.Name[^4..];
//                bool isValidSessionYear = int.TryParse(sessionYearText, out int sessionYear);
//                bool isAdmissionYear = isValidSessionYear && sessionYear == admissionYear;

//                if (isAdmissionYear && cFee.SL is >= 1 and <= 12 && cFee.SL < admissionMonth)
//                {
//                    continue;
//                }

//                // Handle Session Fee and Admission Fee skipping logic
//                string feeHeadName = cFee.StudentFeeHead.Name?.Trim();

//                if (!isAdmissionYear && feeHeadName == "Session Fee")
//                {
//                    continue;
//                }

//                if (isAdmissionYear && feeHeadName == "Admission Fee")
//                {
//                    continue;
//                }

//                var allocation =studentAllAllocations.FirstOrDefault(s => s.StudentFeeHeadId == cFee.StudentFeeHeadId);
//                if (allocation!=null)
//                {
//                    cFee.Amount = allocation.AllocatedAmount;
//                }
//                classFeeLists.Add(cFee);
//            }

//            if (classFeeLists.Count > 0)
//            {
//                totalFees = classFeeLists
//                    .Where(c => c.StudentFeeHead.IsResidential == student.IsResidential)
//                    .Select(s => s.Amount)
//                    .Sum();
//            }
//        }
//        return totalFees;
//    }

//    public async Task<double> GetStudentTotalPaidAmountBySession(int sessionId, string studentUniqueId)
//    {
//        var student = await _studentRepository.GetStudentByUniqueIdAsync(studentUniqueId);
//        var allPayments = await GetPaymentByStudentUniqueId(studentUniqueId);
//        var amount = allPayments.Where(s => s.AcademicSessionId == sessionId).Select(s => s.TotalPayment).Sum();
        
//        return amount;
//    }

//    public async Task<double> GetStudentTotalDueAmountBySession(int sessionId, string studentUniqueId)
//    {
//        var student = await _studentRepository.GetStudentByUniqueIdAsync(studentUniqueId);
//        var currentDues = await GetStudentCurrentDue(student.Id);
//        return currentDues;
//    }
//    private List<ClassFeeList> FilterWithAdmissionSessionFee(string admissionYear, string sessionYear, List<ClassFeeList> classFeeLists)
//    {
//        if (admissionYear == sessionYear)
//        {

//            //Remove Session Fee
//            classFeeLists = classFeeLists.Where(c => !c.StudentFeeHead.Name.Contains("Session Fee")).ToList();
//        }
//        else
//        {
//            //Remove Admission Fee

//        }
//        return classFeeLists;
//    }
//}

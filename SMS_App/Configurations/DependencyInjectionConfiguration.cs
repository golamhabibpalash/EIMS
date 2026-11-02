using Microsoft.Extensions.DependencyInjection;
using Repositories;
using SMS.BLL.Contracts;
using SMS.BLL.Contracts.Reports;
using SMS.BLL.Managers;
using SMS.BLL.Managers.Reports;
using SMS.DAL.Contracts;
using SMS.DAL.Contracts.Reports;
using SMS.DAL.Repositories;
using SMS.DAL.Repositories.Reports;
using SMS_App.Utilities.LoggerService;
using SMS_App.Utilities.ShortMessageService;

namespace SMS_App.Configurations;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection Addservices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeManager, EmployeeManager>();

        services.AddScoped<IAcademicClassRepository, AcademicClassRepository>();
        services.AddScoped<IAcademicClassManager, AcademicClassManager>();

        services.AddScoped<IAcademicSessionRepository, AcademicSessionRepository>();
        services.AddScoped<IAcademicSessionManager, AcademicSessionManager>();

        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IStudentManager, StudentManager>();
        services.AddScoped<IStudentBulkImportManager, StudentBulkImportManager>();

        services.AddScoped<IInstituteRepository, InstituteRepository>();
        services.AddScoped<IInstituteManager, InstituteManager>();

        services.AddScoped<IAcademicSectionRepositoy, AcademicSectionRepository>();
        services.AddScoped<IAcademicSectionManager, AcademicSectionManager>();

        services.AddScoped<IAcademicSubjectRepository, AcademicSubjectRepository>();
        services.AddScoped<IAcademicSubjectManager, AcademicSubjectManager>();

        services.AddScoped<IAcademicSubjectTypeRepository, AcademicSubjectTypeRepository>();
        services.AddScoped<IAcademicSubjectTypeManager, AcademicSubjectTypeManager>();

        services.AddScoped<IDesignationRepository, DesignationRepository>();
        services.AddScoped<IDesignationManager, DesignationManager>();

        services.AddScoped<IDesignationTypeRepository, DesignationTypeRepository>();
        services.AddScoped<IDesignationTypeManager, DesignationTypeManager>();

        services.AddScoped<IEmpTypeRepository, EmpTypeRepository>();
        services.AddScoped<IEmpTypeManager, EmpTypeManager>();

        services.AddScoped<IGenderRepository, GenderRepository>();
        services.AddScoped<IGenderManager, GenderManager>();

        services.AddScoped<INationalityRepository, NationalityRepository>();
        services.AddScoped<INationalityManager, NationalityManager>();

        services.AddScoped<IStudentPaymentRepository, StudentPaymentRepository>();
        services.AddScoped<IStudentPaymentManager, StudentPaymentManager>();

        services.AddScoped<IStudentPaymentDetailsRepository, StudentPaymentDetailsRepository>();
        services.AddScoped<IStudentPaymentDetailsManager, StudentPaymentDetailsManager>();

        services.AddScoped<IStudentFeeHeadRepository, StudentFeeHeadRepository>();
        services.AddScoped<IStudentFeeHeadManager, StudentFeeHeadManager>();

        services.AddScoped<IReligionRepository, ReligionRepository>();
        services.AddScoped<IReligionManager, ReligionManager>();

        services.AddScoped<IDivisionRepository, DivisionRepository>();
        services.AddScoped<IDivisionManager, DivisionManager>();

        services.AddScoped<IDistrictRepository, DistrictRepository>();
        services.AddScoped<IDistrictManager, DistrictManager>();

        services.AddScoped<IUpazilaRepository, UpazilaRepository>();
        services.AddScoped<IUpazilaManager, UpazilaManager>();

        services.AddScoped<IBloodGroupRepository, BloodGroupRepository>();
        services.AddScoped<IBloodGroupManager, BloodGroupManager>();

        services.AddScoped<IClassFeeListRepository, ClassFeeListRepository>();
        services.AddScoped<IClassFeeListManager, ClassFeeListManager>();

        services.AddScoped<IPhoneSMSRepository, PhoneSMSRepository>();
        services.AddScoped<IPhoneSMSManager, PhoneSMSManager>();
        services.AddHttpClient<SmsApiService>();

        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IAttendanceManager, AttendanceManager>();

        services.AddScoped<IAttendanceMachineRepository, AttendanceMachineRepository>();
        services.AddScoped<IAttendanceMachineManager, AttendanceMachineManager>();

        services.AddScoped<IAttendanceMachineDeviceRepository, AttendanceMachineDeviceRepository>();
        services.AddScoped<IAttendanceMachineDeviceManager, AttendanceMachineDeviceManager>();

        //ADMS push protocol (/iclock) - used by push-only terminals such as the SenseFace T1
        services.AddScoped<IAdmsPushManager, AdmsPushManager>();

        //Turns a stored punch into a guardian/employee SMS. Lives in this layer
        //because MobileSMS does; both the /iclock push path and the manual
        //SendSMS endpoint share it.
        services.AddScoped<IAttendanceSmsNotifier, AttendanceSmsNotifier>();

        //Tickets
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<ITicketCommentRepository, TicketCommentRepository>();
        services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();
        services.AddScoped<ITicketManager, TicketManager>();

        //Notifications (in-app notification centre)
        services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
        services.AddScoped<IUserNotificationManager, UserNotificationManager>();
        services.AddScoped<SMS_App.Utilities.Notifications.INotificationService, SMS_App.Utilities.Notifications.NotificationService>();

        services.AddScoped<IZKTecoService, ZKTecoService>();
        services.AddScoped<IAttendanceMachineService, AttendanceMachineService>();

        services.AddScoped<IChapterRepository, ChapterRepository>();
        services.AddScoped<IChapterManager, ChapterManager>();

        services.AddScoped<IQuestionRepository, QuestionRepository>();
        services.AddScoped<IQuestionManager, QuestionManager>();

        services.AddScoped<IQuestionFormationRepository, QuestionFormationRepository>();
        services.AddScoped<IQuestionFormationManager, QuestionFormationManager>();

        services.AddScoped<IStudentActivateHistRepository, StudentActivateHistRepository>();
        services.AddScoped<IStudentActivateHistManager, StudentActivateHistManager>();

        services.AddScoped<IEmployeeActivateHistRepository, EmployeeActivateHistRepository>();
        services.AddScoped<IEmployeeActivateHistManager, EmployeeActivateHistManager>();

        services.AddScoped<ISetupMobileSMSRepository, SetupMobileSMSRepository>();
        services.AddScoped<ISetupMobileSMSManager, SetupMobileSMSManager>();

        services.AddScoped<IOffDayRepository, OffDayRepository>();
        services.AddScoped<IOffDayManager, OffDayManager>();

        services.AddScoped<IOffDayTypeRepository, OffDayTypeRepository>();
        services.AddScoped<IOffDayTypeManager, OffDayTypeManager>();

        services.AddScoped<IAcademicExamTypeRepository, AcademicExamTypeRepository>();
        services.AddScoped<IAcademicExamTypeManager, AcademicExamTypeManager>();

        services.AddScoped<IAcademicExamGroupRepository, AcademicExamGroupRepository>();
        services.AddScoped<IAcademicExamGroupManager, AcademicExamGroupManager>();

        services.AddScoped<IAcademicExamRepository, AcademicExamRepository>();
        services.AddScoped<IAcademicExamManager, AcademicExamManager>();

        services.AddScoped<IAcademicExamDetailsRepository, AcademicExamDetailsRepository>();
        services.AddScoped<IAcademicExamDetailsManager, AcademicExamDetailsManager>();

        services.AddScoped<IExamResultManager, ExamResultManager>();
        services.AddScoped<IExamResultRepository, ExamResultRepository>();

        services.AddScoped<IGradingTableManager, GradingTableManager>();
        services.AddScoped<IGradingTableRepository, GradingTableRepository>();

        services.AddScoped<IStudentFeeAllocationManager, StudentFeeAllocationManager>();
        services.AddScoped<IStudentFeeAllocationRepository, StudentFeeAllocationRepository>();

        services.AddScoped<IAcademicClassSubjectManager, AcademicClassSubjectManager>();
        services.AddScoped<IAcademicClassSubjectRepository, AcademicClassSubjectRepository>();

        services.AddScoped<ISubjectEnrollmentManager, SubjectEnrollmentManager>();
        services.AddScoped<ISubjectEnrollmentRepository, SubjectEnrollmentRepository>();

        services.AddScoped<ISubjectEnrollmentDetailManager, SubjectEnrollmentDetailManager>();
        services.AddScoped<ISubjectEnrollmentDetailRepository, SubjectEnrollmentDetailRepository>();

        services.AddScoped<IClaimStoreManager, ClaimStoreManager>();
        services.AddScoped<IClaimStoreRepository, ClaimStoreRepository>();

        services.AddScoped<IProjectModuleManager, ProjectModuleManager>();
        services.AddScoped<IProjectModuleRepository, ProjectModuleRepository>();

        services.AddScoped<IProjectSubModuleManager, ProjectSubModuleManager>();
        services.AddScoped<IProjectSubModuleRepository, ProjectSubModuleRepository>();

        services.AddScoped<IApplicationSettingsManager, ApplicationSettingsManager>();
        services.AddScoped<IApplicationSettingsRepository, ApplicationSettingsRepository>();

        services.AddScoped<IParamBusConfigManager, ParamBusConfigManager>();
        services.AddScoped<IParamBusConfigRepository, ParamBusConfigRepository>();

        // Online Admission (replaces the retired AppliedStudent prototype; its table is kept for old data)
        services.AddScoped<IAdmissionSettingRepository, AdmissionSettingRepository>();
        services.AddScoped<IAdmissionSessionRepository, AdmissionSessionRepository>();
        services.AddScoped<IAdmissionProgramRepository, AdmissionProgramRepository>();
        services.AddScoped<IAdmissionDocumentRequirementRepository, AdmissionDocumentRequirementRepository>();
        services.AddScoped<IAdmissionGuardianAccountRepository, AdmissionGuardianAccountRepository>();
        services.AddScoped<IAdmissionApplicationRepository, AdmissionApplicationRepository>();
        services.AddScoped<IAdmissionApplicationDocumentRepository, AdmissionApplicationDocumentRepository>();
        services.AddScoped<IAdmissionPaymentRepository, AdmissionPaymentRepository>();
        services.AddScoped<IAdmissionApplicationHistoryRepository, AdmissionApplicationHistoryRepository>();
        services.AddScoped<IAdmissionSetupManager, AdmissionSetupManager>();
        services.AddScoped<IAdmissionGuardianManager, AdmissionGuardianManager>();
        services.AddScoped<IAdmissionApplicationManager, AdmissionApplicationManager>();
        services.AddScoped<IAdmissionPaymentManager, AdmissionPaymentManager>();
        services.AddSingleton<SMS_App.Utilities.OnlineAdmission.AdmissionFileStore>();
        services.AddScoped<SMS_App.Utilities.OnlineAdmission.AdmissionNotifier>();

        services.AddScoped<ILogManager, LogManager>();
        services.AddScoped<ILogRepository, LogRepository>();

        services.AddScoped<IAttachDocManager, AttachDocManager>();
        services.AddScoped<IAttachDocRepository, AttachDocRepository>();

        services.AddScoped<IExpenseTypeRepository, ExpenseTypeRepository>();


        //Reporing part start here===================================
        services.AddScoped<IReportManager, ReportManager>();
        services.AddScoped<IReportRepository, ReportRepository>();


        //Logger =============================
        services.AddScoped<IAppLogger, AppLogger>();
        return services;
    }
}

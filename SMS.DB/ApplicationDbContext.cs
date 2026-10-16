
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.AdditionalModels.Finance;
using SMS.Entities.RptModels;
using SMS.Entities.RptModels.AttendanceVM;
using SMS.Entities.RptModels.Results;
using SMS.Entities.RptModels.StudentPayment;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.DB
{
    public class ApplicationDbContext : IdentityDbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        #region A
        public DbSet<AcademicClass> AcademicClass { get; set; }
        public DbSet<AcademicSection> AcademicSection { get; set; }
        public DbSet<AcademicSession> AcademicSession { get; set; }
        public DbSet<AcademicSubject> AcademicSubject { get; set; }
        public DbSet<AcademicClassSubject> AcademicClassSubjects { get; set; }
        public DbSet<AcademicSubjectType> AcademicSubjectType { get; set; }
        public DbSet<AcademicExamType> AcademicExamTypes { get; set; }
        public DbSet<AcademicExam> AcademicExams { get; set; }
        public DbSet<AcademicExamGroup> AcademicExamGroups { get; set; }
        public DbSet<AcademicExamDetail> AcademicExamDetails { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationSettings> ApplicationSettings { get; set; }
        public DbSet<AppliedStudent> AppliedStudent { get; set; }
        public DbSet<AttachDoc> AttachDocs { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        #endregion

        #region B
        public DbSet<BloodGroup> BloodGroup { get; set; }
        #endregion

        #region C
        public DbSet<ClassFeeList> ClassFeeList { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<ClaimStores> ClaimStores { get; set; }
        public DbSet<ClassRoom> ClassRooms { get; set; }
        public DbSet<ClassRoutine> ClassRoutines { get; set; }
        public DbSet<ClassPeriods> ClassPeriods { get; set; }

        #endregion

        #region D
        public DbSet<Designation> Designation { get; set; }
        public DbSet<DesignationType> DesignationType { get; set; }
        public DbSet<District> District { get; set; }
        public DbSet<Division> Division { get; set; }
        public DbSet<Days> Days { get; set; }
        #endregion

        #region E
        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmployeeActivateHist> EmployeeActivateHists { get; set; }
        public DbSet<EmpType> EmpType { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<ExamResultDetail> ExamResultDetails { get; set; }
        #endregion

        #region F
        #endregion

        #region G
        public DbSet<Gender> Gender { get; set; }
        public DbSet<GradingTable> GradingTables { get; set; }
        public DbSet<GradingTableHist> GradingTableHists { get; set; }
        #endregion

        #region I
        public DbSet<Institute> Institute { get; set; }
        #endregion

        #region L
        public DbSet<Log> Logs { get; set; }
        #endregion

        #region N
        public DbSet<Nationality> Nationality { get; set; }
        public DbSet<NotificationEvent> NotificationEvents { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        #endregion

        #region O
        public DbSet<OffDay> OffDays { get; set; }
        public DbSet<OffDayType> OffDayTypes { get; set; }
        #endregion

        #region P
        public DbSet<ParamBusConfig> ParamBusConfigs { get; set; }
        public DbSet<PhoneSMS> PhoneSMS { get; set; }
        public DbSet<ProjectModule> ProjectModules { get; set; }
        public DbSet<ProjectSubModule> ProjectSubModules { get; set; }
        #endregion

        #region Q
        public DbSet<QuestionFormat> QuestionFormats { get; set; }
        public DbSet<QuestionDetails> QuestionDetails { get; set; }
        public DbSet<Question> Questions { get; set; }
        #endregion

        #region R
        public DbSet<Religion> Religion { get; set; }
        #endregion

        #region S
        public DbSet<SetupMobileSMS> SetupMobileSMSs { get; set; }
        public DbSet<Student> Student { get; set; }
        public DbSet<StudentFeeHead> StudentFeeHead { get; set; }
        public DbSet<StudentPayment> StudentPayment { get; set; }
        public DbSet<StudentPaymentDetails> StudentPaymentDetails { get; set; }
        public DbSet<StudentActivateHist> StudentActivateHists { get; set; }
        public DbSet<StudentFeeAllocation> StudentFeeAllocations { get; set; }
        public DbSet<SubjectEnrollment> SubjectEnrollments { get; set; }
        public DbSet<SubjectEnrollmentDetail> SubjectEnrollmentDetails { get; set; }
        #endregion

        #region T
        public DbSet<Tran_MachineRawPunch> Tran_MachineRawPunch { get; set; }
        public DbSet<TeacherSubjectMap> TeacherSubjectMaps { get; set; }
        #endregion

        #region A_M
        public DbSet<AttendanceMachine> AttendanceMachines { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<TicketComment> TicketComments { get; set; }

        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        #endregion

        #region U
        public DbSet<Upazila> Upazila { get; set; }
        #endregion



        [NotMapped]
        public DbSet<RptStudentVM> RptStudentVMs { get; set; }

        [NotMapped]
        public DbSet<RptAdmitCardVM> RptAdmitCardVMs { get; set; }

        [NotMapped]
        public DbSet<AttendanceVM> AttendanceVMs { get; set; }

        [NotMapped]
        public DbSet<StudentPaymentSummeryVM> StudentPaymentSummeryVMs { get; set; }

        [NotMapped]
        public DbSet<StudentPaymentSummerySMS_VM> studentPaymentSummerySMS_VMs { get; set; }

        [NotMapped]
        public DbSet<rptStudentPaymentsVM> RptStudentPaymetnsVMs { get; set; }

        [NotMapped]
        public DbSet<RptStudentsPaymentVM> RptStudentsPaymetnsVMs { get; set; }
        [NotMapped]
        public DbSet<rptStudentPaymentsVM> RptS { get; set; }
        [NotMapped]
        public DbSet<RptDailyAttendaceVM> RptDailyAttendaceVMs { get; set; }
        [NotMapped]
        public DbSet<RptPaymentReceiptVM> RptPaymentReceiptVMs { get; set; }
        [NotMapped]
        public DbSet<StudentPaymentScheduleVM> StudentPaymentScheduleVMs { get; set; }
        [NotMapped]
        public DbSet<StudentPaymentSchedulePaidVM> StudentPaymentSchedulePaidVMs { get; set; }
        public DbSet<StudentListVM> StudentListVMs { get; set; }
        [NotMapped]
        public DbSet<SubjectWiseMarkSheetVM> SubjectWiseMarkSheetVMs { get; set; }
        [NotMapped]
        public DbSet<StudentWiseMarkSheetVM> StudentWiseMarkSheetVMs { get; set; }

        [NotMapped]
        public DbSet<PaidAmountResult> PaidAmountResults { get; set; }

        [NotMapped]
        public DbSet<PreviouisPaymentDetailsDto> PreviousPaymentsSummery { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ClaimStores>()
                .HasIndex(p => new { p.ClaimValue, p.ClaimType })
                .IsUnique(true);

            builder.Entity<ProjectModule>()
                .HasIndex(p => new { p.ModuleName })
                .IsUnique(true);

            builder.Entity<Student>()
                .HasIndex(s => new { s.UniqueId, s.ClassRoll })
                .IsUnique(true);

            builder.Entity<ApplicationSettings>()
                .HasIndex(s => new { s.ShortName })
                .IsUnique(true);

            builder.Entity<GradingTableHist>()
                .HasIndex(s => s.AcademicExamGroupId)
                .IsUnique();

            builder.Entity<ParamBusConfig>()
                .HasIndex(s => s.ParamSL)
                .IsUnique();

            builder.Entity<ParamBusConfig>()
                .HasIndex(s => s.ConfigName)
                .IsUnique();

            builder.Entity<AttendanceVM>(entity => entity.HasNoKey());
            builder.Entity<StudentPaymentSummeryVM>(entity => entity.HasNoKey());
            builder.Entity<StudentPaymentSummerySMS_VM>(entity => entity.HasNoKey());
            builder.Entity<RptStudentVM>().ToView(nameof(RptStudentVMs)).HasNoKey();
            builder.Entity<rptStudentPaymentsVM>().ToView(nameof(RptStudentPaymetnsVMs)).HasNoKey();
            builder.Entity<RptAdmitCardVM>().ToView(nameof(RptAdmitCardVMs)).HasNoKey();
            builder.Entity<RptStudentsPaymentVM>().ToView(nameof(RptStudentsPaymetnsVMs)).HasNoKey();
            builder.Entity<RptDailyAttendaceVM>().ToView(nameof(RptDailyAttendaceVMs)).HasNoKey();
            builder.Entity<StudentListVM>().ToView(nameof(StudentListVMs)).HasNoKey();
            builder.Entity<RptPaymentReceiptVM>().ToView(nameof(RptPaymentReceiptVMs)).HasNoKey();
            builder.Entity<StudentPaymentScheduleVM>().ToView(nameof(StudentPaymentScheduleVMs)).HasNoKey();
            builder.Entity<StudentPaymentSchedulePaidVM>().ToView(nameof(StudentPaymentSchedulePaidVMs)).HasNoKey();
            builder.Entity<SubjectWiseMarkSheetVM>().ToView(nameof(SubjectWiseMarkSheetVMs)).HasNoKey();
            builder.Entity<StudentWiseMarkSheetVM>().ToView(nameof(StudentWiseMarkSheetVMs)).HasNoKey();
            builder.Entity<PaidAmountResult>().ToView(nameof(PaidAmountResults)).HasNoKey();
            builder.Entity<PreviouisPaymentDetailsDto>().ToView(nameof(PreviousPaymentsSummery)).HasNoKey();

            builder.Entity<Employee>(entity =>
            {
                entity.HasOne(e => e.PresentDistrict)
                    .WithMany()
                    .HasForeignKey(e => e.PresentDistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PermanentDistrict)
                    .WithMany()
                    .HasForeignKey(e => e.PermanentDistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PresentDivision)
                    .WithMany()
                    .HasForeignKey(e => e.PresentDivisionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PermanentDivision)
                    .WithMany()
                    .HasForeignKey(e => e.PermanentDivisionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PresentUpazila)
                    .WithMany()
                    .HasForeignKey(e => e.PresentUpazilaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PermanentUpazila)
                    .WithMany()
                    .HasForeignKey(e => e.PermanentUpazilaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Student>(entity =>
            {
                entity.HasOne(e => e.PresentDistrict)
                    .WithMany()
                    .HasForeignKey(e => e.PresentDistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PermanentDistrict)
                    .WithMany()
                    .HasForeignKey(e => e.PermanentDistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PresentDivision)
                    .WithMany()
                    .HasForeignKey(e => e.PresentDivisionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PermanentDivision)
                    .WithMany()
                    .HasForeignKey(e => e.PermanentDivisionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PresentUpazila)
                    .WithMany()
                    .HasForeignKey(e => e.PresentUpazilaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PermanentUpazila)
                    .WithMany()
                    .HasForeignKey(e => e.PermanentUpazilaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<AppliedStudent>(entity =>
            {
                entity.HasOne(e => e.PresentDistrict)
                    .WithMany()
                    .HasForeignKey(e => e.PresentDistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PermanentDistrict)
                    .WithMany()
                    .HasForeignKey(e => e.PermanentDistrictId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PresentDivision)
                    .WithMany()
                    .HasForeignKey(e => e.PresentDivisionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PermanentDivision)
                    .WithMany()
                    .HasForeignKey(e => e.PermanentDivisionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PresentUpazila)
                    .WithMany()
                    .HasForeignKey(e => e.PresentUpazilaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PermanentUpazila)
                    .WithMany()
                    .HasForeignKey(e => e.PermanentUpazilaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    if (foreignKey.DeleteBehavior == DeleteBehavior.Cascade)
                    {
                        foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                    }
                }
            }

        }
    }
}

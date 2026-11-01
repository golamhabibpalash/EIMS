using AutoMapper;
using SMS_App.ViewModels.ClaimContext;
using SMS_App.ViewModels.Employees;
using SMS_App.ViewModels.ExamVM;
using SMS_App.ViewModels.ModuleSubModuleVM;
using SMS_App.ViewModels.SetupVM;
using SMS_App.ViewModels.Students;
using SMS.Entities;
using SMS_App.ViewModels.LogVM;
using SMS_App.ViewModels.Testimonials;
using SMS_App.ViewModels.TransferCertificates;

namespace SMS_App.Utilities.AutoMapperConfiguration
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<StudentCreateVM, Student>();
            CreateMap<Student, StudentCreateVM>();

            CreateMap<StudentEditVM, Student>();
            CreateMap<Student, StudentEditVM>();

            CreateMap<StudentListVM, Student>();
            CreateMap<Student, StudentListVM>();

            // Testimonials: ignore navigation/audit on the way into the entity.
            CreateMap<TestimonialCreateVM, Testimonial>()
                .ForMember(d => d.Student, o => o.Ignore());
            CreateMap<TestimonialEditVM, Testimonial>()
                .ForMember(d => d.Student, o => o.Ignore());
            CreateMap<Testimonial, TestimonialEditVM>();

            // Transfer Certificates.
            CreateMap<TransferCertificateCreateVM, TransferCertificate>()
                .ForMember(d => d.Student, o => o.Ignore());
            CreateMap<TransferCertificateEditVM, TransferCertificate>()
                .ForMember(d => d.Student, o => o.Ignore());
            CreateMap<TransferCertificate, TransferCertificateEditVM>();

            CreateMap<EmployeeCreateVM, Employee>();
            CreateMap<Employee, EmployeeCreateVM>();

            CreateMap<EmployeeEditVM, Employee>();
            CreateMap<Employee, EmployeeEditVM>();

            CreateMap<EmployeeDetailsVM, Employee>();
            CreateMap<Employee, EmployeeDetailsVM>();

            CreateMap<AcademicExam, AcademicExamVM>().ReverseMap();

            CreateMap<AcademicExamGroup, AcademicExamGroupIndexVM>().ReverseMap();

            CreateMap<SetupMobileSMS, AttendanceSetupVM>().ReverseMap();

            CreateMap<ProjectSubModule, ProjectSubModuleVM>().ReverseMap();

            CreateMap<ClaimStores, ClaimStoreVM>().ReverseMap();

            CreateMap<AcademicExam, AcademicExamDetailVM>().ReverseMap();

            CreateMap<Log, ActivityLogVM>().ReverseMap();

        }
    }
}

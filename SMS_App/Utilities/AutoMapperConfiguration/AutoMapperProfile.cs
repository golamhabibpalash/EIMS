using AutoMapper;
using SMS_App.ViewModels.ClaimContext;
using SMS_App.ViewModels.Employees;
using SMS_App.ViewModels.ExamVM;
using SMS_App.ViewModels.ModuleSubModuleVM;
using SMS_App.ViewModels.SetupVM;
using SMS_App.ViewModels.Students;
using SMS.Entities;
using SMS_App.ViewModels.LogVM;
using SMS_App.ViewModels.InstituteVM;

namespace SMS_App.Utilities.AutoMapperConfiguration
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Image files are saved by InstitutesController; only their stored names reach the entity.
            CreateMap<InstituteCreateVM, Institute>()
                .ForMember(d => d.Logo, o => o.Ignore())
                .ForMember(d => d.Banner, o => o.Ignore())
                .ForMember(d => d.FavIcon, o => o.Ignore());

            CreateMap<StudentCreateVM, Student>();
            CreateMap<Student, StudentCreateVM>();

            CreateMap<StudentEditVM, Student>();
            CreateMap<Student, StudentEditVM>();

            CreateMap<StudentListVM, Student>();
            CreateMap<Student, StudentListVM>();

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

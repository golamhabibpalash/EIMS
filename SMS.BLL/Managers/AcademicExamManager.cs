using BLL.Managers.Base;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels;
using SMS.Entities.AdditionalModels.StudentVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers;

public class AcademicExamManager : Manager<AcademicExam>, IAcademicExamManager
{
    private readonly IAcademicExamRepository _academicExamRepository;
    private readonly IAcademicSessionRepository _academicSessionRepository;
    private readonly IAcademicExamGroupRepository _academicExamGroupRepository;
    private readonly IAcademicClassRepository _academicClassRepository;
    private readonly IGradingTableRepository _gradingTableRepository;
    private readonly IExamResultManager _examResultManager;
    private readonly IAttendanceMachineManager _attendanceMachineManager;
    private readonly IOffDayManager _offDayManager;
    private AcademicExamGroup _cachedExamGroup;
    private List<GradingTable> _cachedGradingTable;
    private int _cachedExamMonthId;


    public AcademicExamManager(
        IAcademicExamRepository academicExamRepository,
        IAcademicSessionRepository academicSessionRepository,
        IAcademicExamGroupRepository academicExamGroupRepository,
        IAcademicClassRepository academicClassRepository,
        IGradingTableRepository gradingTableRepository,
        IExamResultManager examResultManager,
        IAttendanceMachineManager attendanceMachineManager,
        IOffDayManager offDayManager) : base(academicExamRepository)
    {
        _academicExamRepository = academicExamRepository;
        _academicSessionRepository = academicSessionRepository;
        _academicExamGroupRepository = academicExamGroupRepository;
        _academicClassRepository = academicClassRepository;
        _gradingTableRepository = gradingTableRepository;
        _examResultManager = examResultManager;
        _attendanceMachineManager = attendanceMachineManager;
        _offDayManager = offDayManager;
    }

    public async Task<List<AcademicExam>> GetByClassIdExamGroupIdAsync(int examGroupId, int academicClassId)
    {
        var result = await _academicExamRepository.GetByClassIdExamGroupId(examGroupId, academicClassId);
        return result;
    }

    public async Task<List<AcademicExam>> GetAllByClassIdExamGroupIdAsync(int examGroupId, int academicClassId)
    {
        var result = await _academicExamRepository.GetAllByClassIdExamGroupId(examGroupId, academicClassId);
        return result;
    }

    public async Task<List<AcademicExam>> GetByClassIdExamGroupIdSectionIdAsync(int examGroupId, int academicClassId, int sectionId)
    {
        var result = await _academicExamRepository.GetByClassIdExamGroupIdSectionId(examGroupId, academicClassId, sectionId);
        return result;
    }

    public async Task<List<ExamSessionDto>> GetExaminationListAsync()
    {
        var allSessions = await _academicSessionRepository.GetAllAsync();
        var allExaminations = await _academicExamRepository.GetAllAsync();

        var result = new List<ExamSessionDto>();

        foreach (var session in allSessions)
        {
            var sessionDto = new ExamSessionDto
            {
                Id = session.Id,
                SessionName = session.Name,
                ExamGroupDtos = new List<ExamGroupDto>()
            };

            var examsForSession = allExaminations
                .Where(e => e.AcademicExamGroup.AcademicSessionId == session.Id)
                .GroupBy(e => e.AcademicExamGroupId);

            foreach (var examGroup in examsForSession)
            {
                var targetedExams = allExaminations.Where(s => s.AcademicExamGroupId == examGroup.Key);
                ExamGroupDto examGroupDto = new ExamGroupDto();
                examGroupDto.Id = examGroup.Key;
                examGroupDto.Name = examGroup.FirstOrDefault().AcademicExamGroup.ExamGroupName;
                examGroupDto.ExaminationDtos = targetedExams.Select(s => MapToExaminationDto(s)).ToList();
                sessionDto.ExamGroupDtos.Add(examGroupDto);
            }

            result.Add(sessionDto);
        }

        return result;
    }

    public async Task<List<ExamSessionDto>> GetExaminationListLiteAsync()
    {
        var allSessions = await _academicSessionRepository.GetAllAsync();
        var allExaminations = await _academicExamRepository.GetAllLiteAsync();

        var result = new List<ExamSessionDto>();

        foreach (var session in allSessions)
        {
            var sessionDto = new ExamSessionDto
            {
                Id = session.Id,
                SessionName = session.Name,
                ExamGroupDtos = new List<ExamGroupDto>()
            };

            var examsForSession = allExaminations
                .Where(e => e.AcademicExamGroup.AcademicSessionId == session.Id)
                .GroupBy(e => e.AcademicExamGroupId);

            foreach (var examGroup in examsForSession)
            {
                var targetedExams = allExaminations.Where(s => s.AcademicExamGroupId == examGroup.Key);
                ExamGroupDto examGroupDto = new ExamGroupDto();
                examGroupDto.Id = examGroup.Key;
                examGroupDto.Name = examGroup.FirstOrDefault().AcademicExamGroup.ExamGroupName;
                examGroupDto.ExaminationDtos = targetedExams.Select(s => MapToExaminationDtoLite(s)).ToList();
                sessionDto.ExamGroupDtos.Add(examGroupDto);
            }

            if (sessionDto.ExamGroupDtos.Any())
            {
                result.Add(sessionDto);
            }
        }

        return result;
    }

    private ExaminationDto MapToExaminationDtoLite(AcademicExam exam)
    {
        return new ExaminationDto
        {
            Id = exam.Id,
            ClassName = exam.AcademicClass?.Name,
            ClassId = exam.AcademicClassId,
            ExamGroupVMId = exam.AcademicExamGroupId,
            ExamCategory = exam.ExamCategory,
            ExamGroupDto = new ExamGroupDto
            {
                Id = exam.AcademicExamGroupId,
                Name = exam.AcademicExamGroup?.ExamGroupName
            },
            ExaminationDetailsDtos = new List<ExaminationDetailsDto>
            {
                new ExaminationDetailsDto
                {
                    Id = exam.Id,
                    SubjectName = exam.AcademicSubject?.SubjectName,
                    SubjectId = exam.AcademicSubjectId,
                    SectionName = exam.AcademicSection?.Name,
                    SectionId = exam.AcademicSectionId,
                    EmployeeName = exam.Employee?.EmployeeName,
                    EmployeeId = exam.EmployeeId,
                    TotalStudents = exam.AcademicExamDetails?.Count ?? 0,
                    TotalMarks = exam.TotalMarks,
                    IsLocked = false
                }
            }
        };
    }
    public async Task<LiveResultVM> GetLiveResultByGroupIdClassIdSectionId(int academicGroupId, int academiClassId, int? academicSectionId)
    {
        var liveResultVM = new LiveResultVM();

        var examGroup = await _academicExamGroupRepository.GetByIdAsync(academicGroupId);
        if (examGroup == null)
        {
            return liveResultVM;
        }
        examGroup.AcademicExams = examGroup.AcademicExams.Where(s => s.AcademicClassId == academiClassId).ToList();
        if (academicSectionId.HasValue && academicSectionId > 0)
        {
            examGroup.AcademicExams = examGroup
                .AcademicExams
                .Where(s => s.AcademicSectionId == academicSectionId.Value)
                .OrderBy(s => s.AcademicExamDetails.Min(e => e.Student.ClassRoll))
                .ToList();

        }
        else
        {
            examGroup.AcademicExams = examGroup
                .AcademicExams
                .Where(s => s.AcademicSectionId == null)
                .OrderBy(s => s.AcademicExamDetails.Min(e => e.Student.ClassRoll))
                .ToList();
        }
        _cachedGradingTable = (List<GradingTable>)await _gradingTableRepository.GetAllAsync();
        _cachedExamGroup = examGroup;
        _cachedExamMonthId = examGroup.ExamMonthId;
        var existingExams = examGroup.AcademicExams.Where(s => s.AcademicClassId == academiClassId).ToList();
        if (existingExams == null || existingExams.Count <= 0)
        {
            return liveResultVM;
        }
        var academicClass = await _academicClassRepository.GetByIdAsync(academiClassId);

        var existingStudents = existingExams.SelectMany(s => s.AcademicExamDetails.OrderBy(e => e.StudentId)).Select(s => s.Student).DistinctBy(s => s.Id).OrderBy(s => s.ClassRoll).ToList();

        if (existingStudents != null)
        {
            var countOfExam = _cachedExamGroup.AcademicExams.DistinctBy(s => s.AcademicSubjectId).Count();
            var studentIds = existingStudents.Select(s => s.Id).ToList();
            var previousRanks = new Dictionary<int, int>();
            if (_examResultManager != null)
            {
                previousRanks = await _examResultManager.GetPreviousRanksByStudentIdsAsync(academicGroupId, studentIds);
            }

            foreach (var student in existingStudents)
            {
                var liveResultSubjectWises = GetLiveResultSubjectWise(student.Id);

                var sumOfSubjectWiseGP = liveResultSubjectWises.Sum(s => s.GPA);
                var isFailedAnySubject = liveResultSubjectWises.Any(s => s.GPA == 0);
                var totalFails = liveResultSubjectWises.Count(c => c.GPA == 0);
                var finalGpa = isFailedAnySubject ? 0 : (sumOfSubjectWiseGP / countOfExam);
                string rStatus = string.Empty;
                LiveResultDetailsVM liveResultDetailsVM = new LiveResultDetailsVM
                {
                    ClassRoll = student.ClassRoll.ToString(),
                    StudentName = student.Name,
                    FinalGPA = finalGpa,
                    FinalGrade = GetFinalGradeByGPA(finalGpa, out rStatus),
                    TotalMarks = GetTotalMarks(student.Id),
                    Attendance = GetAttendance(student.Id, academicGroupId),
                    Rank = 0,
                    PreviousRank = previousRanks.ContainsKey(student.Id) ? previousRanks[student.Id] : 0,
                    Fails = totalFails,
                    Status = rStatus,
                    LiveResultSubjectWises = liveResultSubjectWises
                };
                liveResultVM.ResultDetails.Add(liveResultDetailsVM);
            }
        }

        if (existingExams != null || existingExams.Count > 0)
        {
            liveResultVM.ExamTitle = $"{examGroup.ExamGroupName} ({academicClass.Name})";

            var totalSubjects = GetTableHeaderSubjects(existingExams);
            liveResultVM.Subjects = totalSubjects;
            int totalExamTypes = totalSubjects.Sum(s => s.ExamTypes.Count);
            liveResultVM.TotalColumn = 10 + (totalSubjects.Count * 2) + totalExamTypes;
        }

        // Calculate Ranking - Order by: fails (ascending), then GPA (desc), then TotalMarks (desc), then Attendance (desc)
        var orderedResults = liveResultVM.ResultDetails
            .OrderBy(r => r.Fails)
            .ThenByDescending(r => r.FinalGPA)
            .ThenByDescending(r => r.TotalMarks)
            .ThenByDescending(r => r.Attendance)
            .ThenBy(r => r.ClassRoll)
            .ToList();

        for (int i = 0; i < orderedResults.Count; i++)
        {
            orderedResults[i].Rank = i + 1;
        }


        return liveResultVM;
    }

    private string GetFinalGradeByGPA(double gpa, out string status)
    {
        var grade = "";
        var gradingTableRow = _cachedGradingTable?.Where(s => (double)s.GradePoint >= gpa).OrderBy(g => g.GradePoint).FirstOrDefault();
        if (gradingTableRow != null)
        {
            grade = gradingTableRow.LetterGrade ?? "";
            status = gradingTableRow.gradeComments ?? "";
        }
        else
        {
            status = "";
        }
        return grade;
    }

    private double GetAttendance(int studentId, int academicGroupId)
    {
        try
        {
            if (_cachedExamMonthId <= 0)
                return 0;

            int year = DateTime.Now.Year;
            int monthId = _cachedExamMonthId;

            DateTime startDate = new DateTime(year, monthId, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            string monthYear = monthId.ToString().PadLeft(2, '0') + year.ToString();

            var attendanceList = _attendanceMachineManager.GetAttendanceByMonthSingleStudent(studentId, monthYear).Result;
            var monthlyHolidays = _offDayManager.GetMonthlyHolidaysAsync(startDate.ToString("MMyyyy")).Result;

            int totalDaysInMonth = DateTime.DaysInMonth(year, monthId);
            int totalActiveDays = totalDaysInMonth - monthlyHolidays.Count;

            if (totalActiveDays <= 0)
                return 0;

            int totalPresent = attendanceList?.Count ?? 0;
            return Math.Round((totalPresent * 100.0) / totalActiveDays, 2);
        }
        catch
        {
            return 0;
        }
    }

    private double GetTotalMarks(int id)
    {
        var result = 0.0;
        foreach (var exam in _cachedExamGroup.AcademicExams)
        {
            var examDetail = exam.AcademicExamDetails.FirstOrDefault(s => s.StudentId == id);
            if (examDetail != null)
            {
                result += examDetail.ObtainMark;
            }
        }
        return result;
    }

    private double GetSubjectWiseTotalMark(int subjectId, int classId)
    {
        var totalMarks = _cachedExamGroup.AcademicExams.Where(e => e.AcademicSubjectId == subjectId && classId == e.AcademicClassId).Sum(e => e.TotalMarks);
        return totalMarks;
    }

    private List<TableHeaderSubjects> GetTableHeaderSubjects(List<AcademicExam> existingExams)
    {
        var classId = existingExams.Select(s => s.AcademicClassId).FirstOrDefault();
        var subjectList = new List<TableHeaderSubjects>();
        foreach (var item in existingExams)
        {
            bool isExist = subjectList.Any(s => s.SubjectName == item.AcademicSubject.SubjectName);
            if (isExist)
            {
                continue;
            }
            TableHeaderSubjects headerSubjects = new TableHeaderSubjects()
            {
                SubjectName = item.AcademicSubject.SubjectName,
                TotalMarks = GetSubjectWiseTotalMark(item.AcademicSubjectId, classId),
                ExamTypes = GetExamTypes(existingExams, item.AcademicSubjectId),
                HighestMark = GetHighestMarkOfTheSubject(item.AcademicSubjectId, classId)
            };
            subjectList.Add(headerSubjects);
        }
        return subjectList;
    }

    private double GetHighestMarkOfTheSubject(int academicSubjectId, int classId)
    {
        var exams = _cachedExamGroup?.AcademicExams?
            .Where(e => e.AcademicSubjectId == academicSubjectId && e.AcademicClassId == classId)
            .ToList();

        if (exams == null || !exams.Any())
            return 0;

        return exams
            .SelectMany(e => e.AcademicExamDetails ?? Enumerable.Empty<AcademicExamDetail>())
            .GroupBy(d => d.StudentId)
            .Select(g => g.Sum(d => d.ObtainMark))
            .DefaultIfEmpty(0)
            .Max();
    }

    private List<TableHeaderExamTypes> GetExamTypes(List<AcademicExam> existingExams, int subjectId)
    {
        var types = new List<TableHeaderExamTypes>();
        var targetedExams = existingExams.Where(s => s.AcademicSubjectId == subjectId);
        foreach (var item in targetedExams)
        {
            TableHeaderExamTypes examType = new TableHeaderExamTypes()
            {
                ExamType = item.ExamCategory ?? "Written",
                TotalMarks = item.TotalMarks,
            };
            types.Add(examType);
        }
        return types;
    }
    
    private double GetPassMark(double totalMark)
    {
        double passMark = 0;
        var maxFailNumber = _cachedGradingTable.FirstOrDefault(s => s.NumberRangeMin == 0).NumberRangeMax;
        if (maxFailNumber > 0)
        {
            passMark = (maxFailNumber * totalMark) / 100;
        }
        return passMark + 1;
    }
    
    private List<LiveResultSubjectWise> GetLiveResultSubjectWise(int studentId)
    {
        var liveResultSubjectWise = new List<LiveResultSubjectWise>();
        if (_cachedExamGroup.AcademicExams.Count > 0)
        {
            var examList = _cachedExamGroup.AcademicExams.DistinctBy(s => s.AcademicSubjectId).ToList();

            foreach (var exam in examList)
            {
                var subWiseObtainMark = GetSubjectWiseTotalObtainMarks(exam.AcademicSubjectId, studentId);
                var totalMarks = GetSubjectWiseTotalMarks(exam.AcademicSubjectId);
                var isFailAnyCategory = IsFailedAnyCategory(exam.AcademicSubjectId, studentId);
                LiveResultSubjectWise lrsw = new()
                {
                    SubjectName = exam.AcademicSubject.SubjectName,
                    Marks = totalMarks,
                    GPA = isFailAnyCategory == true ? 0 : GetGPAForSingleSubject(subWiseObtainMark, totalMarks),
                    SubjectTypes = GetLiveResultSubjectType(exam.AcademicSubjectId, studentId),
                    ObtainMarks = subWiseObtainMark,
                };
                lrsw.TotalColumn = 2 + lrsw.SubjectTypes.Count;
                var isExistOnSubjectWise = liveResultSubjectWise.FirstOrDefault(s => s.SubjectName == lrsw.SubjectName);
                if (isExistOnSubjectWise == null)
                {
                    liveResultSubjectWise.Add(lrsw);
                }
            }
        }
        return liveResultSubjectWise;
    }

    private bool IsFailedAnyCategory(int academicSubjectId, int studentId)
    {
        var result = false;
        var allExams = _cachedExamGroup.AcademicExams.Where(s => s.AcademicSubjectId == academicSubjectId).ToList();
        if (allExams != null && allExams.Count > 0)
        {
            foreach (var exam in allExams)
            {
                if (exam.AcademicExamDetails?.Count <= 0)
                {
                    continue;
                }
                var examDetail = exam.AcademicExamDetails.FirstOrDefault(s => s.StudentId == studentId);
                if (examDetail == null)
                {
                    continue;
                }
                var hundredPercentMark = (examDetail.ObtainMark * 100) / exam.TotalMarks;

                //Compare and Calculation Grade Point            
                var gradePoint = _cachedGradingTable
                    .FirstOrDefault(g => hundredPercentMark >= g.NumberRangeMin && hundredPercentMark <= g.NumberRangeMax)?
                    .GradePoint ?? 0m;
                if (gradePoint <= 0)
                {
                    result = true;
                    break;
                }
            }
        }
        return result;
    }

    private double GetSubjectWiseTotalMarks(int subjectId)
    {
        var result = 0.0;
        var ss = _cachedExamGroup.AcademicExams.Where(s => s.AcademicSubjectId == subjectId).ToList();
        result = ss?.Sum(s => s.TotalMarks) ?? result;
        return result;
    }

    private double GetGPAForSingleSubject(double obtainMark, double totalMarks)
    {
        //Make 100%
        var hundredPercentMark = (obtainMark * 100) / totalMarks;

        //Compare and Calculation Grade Point            
        var gradePoint = _cachedGradingTable
            .FirstOrDefault(g => hundredPercentMark >= g.NumberRangeMin && hundredPercentMark <= g.NumberRangeMax)?
            .GradePoint ?? 0m;

        return (double)gradePoint;
    }

    private double GetSubjectWiseTotalObtainMarks(int examId, int studentId)
    {
        var result = 0.0;
        result = _cachedExamGroup.AcademicExams.SelectMany(s => s.AcademicExamDetails.Where(e => e.AcademicExam.AcademicSubjectId == examId && e.StudentId == studentId)).Sum(c => c.ObtainMark);

        return result;
    }

    private List<LiveResultSubjectType> GetLiveResultSubjectType(int academicSubjectId, int studentId)
    {
        var liveResultSubjectTypes = new List<LiveResultSubjectType>();
        var existingExams = _cachedExamGroup.AcademicExams.Where(s => s.AcademicSubjectId == academicSubjectId).ToList();
        foreach (var item in existingExams)
        {
            LiveResultSubjectType liveResultSubjectType = new LiveResultSubjectType()
            {
                SubjectTypeName = item.ExamCategory,
                TotalMarks = item.TotalMarks,
                GetMarks = item.AcademicExamDetails.FirstOrDefault(s => s.StudentId == studentId)?.ObtainMark ?? 0,
                PassMark = GetPassMark(item.TotalMarks)
            }
        ;
            var isExist = liveResultSubjectTypes.Any(s => s.SubjectTypeName == liveResultSubjectType.SubjectTypeName);
            if (!isExist)
            {
                liveResultSubjectTypes.Add(liveResultSubjectType);
            }
        }
        return liveResultSubjectTypes;
    }

    private ExaminationDto MapToExaminationDto(AcademicExam exam)
    {
        return new ExaminationDto
        {
            Id = exam.Id,
            ClassName = exam.AcademicClass.Name,
            ClassId = exam.AcademicClassId,
            ExamGroupVMId = exam.AcademicExamGroupId,
            ExamCategory = exam.ExamCategory,
            // Fix: prevent duplicate details
            ExaminationDetailsDtos = exam.AcademicExamDetails
                .GroupBy(d => new { exam.AcademicSubjectId, exam.AcademicSectionId, exam.EmployeeId })
                .Select(g => new ExaminationDetailsDto
                {
                    Id = g.First().Id,
                    SubjectName = exam.AcademicSubject.SubjectName,
                    SubjectId = exam.AcademicSubjectId,
                    SectionName = exam.AcademicSection?.Name,
                    SectionId = exam.AcademicSection?.Id,
                    EmployeeName = exam.Employee.EmployeeName,
                    EmployeeId = exam.EmployeeId,
                    TotalStudents = g.Count(),
                    TotalMarks = exam.TotalMarks,
                    IsLocked = g.First().Status
                }).ToList()
        };
    }

    public async Task<AcademicExam> GetAcademicExam(int examGroupId, int classId, int subjectId, string examCategory, int? sectionId)
    {
        var existingExam = await _repository.Table
        .FirstOrDefaultAsync(s =>
            s.AcademicExamGroupId == examGroupId &&
            s.AcademicClassId == classId &&
            s.AcademicSubjectId == subjectId &&
            s.ExamCategory == examCategory &&
            s.AcademicSectionId == sectionId
        );

        return existingExam;
    }
    public async Task<int> GetTotalExamAsync(int examGroupId, int classId)
    {
        int totalExamsCount = 0;
        var totalExams = await _repository.Table
            .Where(s => s.AcademicExamGroupId == examGroupId && s.AcademicClassId == classId)
            .GroupBy(e => e.AcademicSubjectId)
            .Select(g => g.First())
            .ToListAsync();
        if (totalExams.Any())
        {
            totalExamsCount = totalExams.Count;
        }
        return totalExamsCount;
    }

    public async Task<List<AcademicClass>> GetAcademicClassListByGroupIdAsync(int groupId)
    {
        List<AcademicClass> classes = new List<AcademicClass>();
        classes = await _repository.Table.Where(s => s.AcademicExamGroupId == groupId).Select(s => s.AcademicClass).DistinctBy(c => c.Id).ToListAsync();
        
        return classes;
    }
    public async Task<ProfileResult> GetSingleStudentResultDetailForProfile(int studentId)
    {
        var profileResult = new ProfileResult();
        var results = await _examResultManager.GetByStudentIdAsync(studentId);
        if (results == null || results.Count == 0)
        {
            return profileResult;
        }

        var orderedResults = results
            .Where(r => r.AcademicExamGroup != null)
            .OrderBy(r => r.AcademicExamGroup.AcademicSession?.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.AcademicExamGroup.AcademicSessionId)
            .ThenBy(r => r.AcademicExamGroup.ExamMonthId)
            .ThenBy(r => r.Id)
            .ToList();

        if (orderedResults.Count == 0)
        {
            return profileResult;
        }

        var latestResult = orderedResults[orderedResults.Count - 1];
        profileResult.CurrentExamName = latestResult.AcademicExamGroup?.ExamGroupName ?? profileResult.CurrentExamName;
        profileResult.CurrentCGPA = latestResult.CGPA;
        profileResult.CurrentClassRank = latestResult.Rank;
        profileResult.CurrentGrade = latestResult.FinalGrade;
        profileResult.CurrentObtainMarks = latestResult.TotalObtainMarks;
        profileResult.CurrentTotalFail = latestResult.TotalFails;

        profileResult.CurrentExamDetails = (latestResult.ExamResultDetails ?? new List<ExamResultDetail>())
            .Where(d => d.AcademicSubject != null)
            .Select(d => new CurrentExamDetail
            {
                SubjectName = d.AcademicSubject.SubjectName,
                SubjectCode = d.AcademicSubject.SubjectCode?.ToString(),
                TotalMark = d.TotalMark,
                TotalObtainMark = d.ObtainMark,
                Grade = d.Grade,
                GPA = d.GPA
            })
            .ToList();

        if (orderedResults.Count > 1)
        {
            profileResult.LastSemesterCGPA = orderedResults[orderedResults.Count - 2].CGPA;
        }

        var sessionGroups = orderedResults
            .GroupBy(r => new
            {
                r.AcademicExamGroup.AcademicSessionId,
                SessionName = r.AcademicExamGroup.AcademicSession?.Name
                    ?? $"Session {r.AcademicExamGroup.AcademicSessionId}"
            })
            .OrderBy(g => g.Key.SessionName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(g => g.Key.AcademicSessionId);

        foreach (var sessionGroup in sessionGroups)
        {
            var sessionResults = sessionGroup
                .OrderBy(r => r.AcademicExamGroup.ExamMonthId)
                .ThenBy(r => r.Id)
                .ToList();
            var latestInSession = sessionResults[sessionResults.Count - 1];

            var session = new ResultSessionProgress
            {
                AcademicSessionId = sessionGroup.Key.AcademicSessionId,
                SessionName = sessionGroup.Key.SessionName,
                ClassName = latestInSession.AcademicClass?.Name,
                ExamName = latestInSession.AcademicExamGroup?.ExamGroupName,
                ExamCount = sessionResults.Count,
                CGPA = latestInSession.CGPA,
                Grade = latestInSession.FinalGrade,
                Rank = latestInSession.Rank,
                TotalFails = latestInSession.TotalFails
            };

            var subjectGroups = sessionResults
                .SelectMany(r => r.ExamResultDetails ?? new List<ExamResultDetail>())
                .Where(d => d.AcademicSubject != null && !string.IsNullOrWhiteSpace(d.AcademicSubject.SubjectName))
                .GroupBy(d => d.AcademicSubject.SubjectName.Trim(), StringComparer.OrdinalIgnoreCase);

            foreach (var subjectGroup in subjectGroups)
            {
                var marks = subjectGroup.OrderBy(d => d.Id).ToList();
                var latestMark = marks[marks.Count - 1];
                session.Subjects.Add(new ResultSubjectMark
                {
                    SubjectName = latestMark.AcademicSubject.SubjectName,
                    SubjectCode = latestMark.AcademicSubject.SubjectCode?.ToString(),
                    ObtainedMarks = marks.Sum(m => m.ObtainMark),
                    TotalMarks = marks.Sum(m => m.TotalMark),
                    Grade = latestMark.Grade,
                    GPA = latestMark.GPA
                });
            }

            session.ObtainedMarks = session.Subjects.Sum(s => s.ObtainedMarks);
            session.TotalMarks = session.Subjects.Sum(s => s.TotalMarks);
            profileResult.Sessions.Add(session);
        }

        var subjectNames = profileResult.Sessions
            .SelectMany(s => s.Subjects)
            .Select(s => s.SubjectName)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var subjectName in subjectNames)
        {
            var trend = new ResultSubjectTrend { SubjectName = subjectName };
            foreach (var session in profileResult.Sessions)
            {
                var mark = session.Subjects.FirstOrDefault(s =>
                    string.Equals(s.SubjectName, subjectName, StringComparison.OrdinalIgnoreCase));
                trend.Percentages.Add(mark?.Percentage);
                trend.Grades.Add(mark?.Grade);
                if (string.IsNullOrWhiteSpace(trend.SubjectCode) && mark != null)
                {
                    trend.SubjectCode = mark.SubjectCode;
                }
            }
            profileResult.SubjectTrends.Add(trend);
        }

        profileResult.SubjectTrends = profileResult.SubjectTrends
            .OrderBy(t => t.SubjectName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return profileResult;
    }

    public async Task<bool> IsDuplicateAsync(int examGroupId, int classId, int subjectId, int? sectionId, string examCategory)
    {
        return await _academicExamRepository.IsDuplicateAsync(examGroupId, classId, subjectId, sectionId, examCategory);
    }

    public async Task<List<(int SectionId, bool IsDuplicate)>> CheckDuplicatesBulkAsync(int examGroupId, int classId, int subjectId, string examCategory, List<int> sectionIds)
    {
        return await _academicExamRepository.CheckDuplicatesBulkAsync(examGroupId, classId, subjectId, examCategory, sectionIds);
    }

    public async Task<List<AcademicExam>> GetMergedExamsAsync(int examId)
    {
        var primaryExam = await _repository.GetByIdAsync(examId);
        if (primaryExam == null)
            return new List<AcademicExam>();

        var allExams = await _repository.GetAllAsync();

        var mergedExams = allExams
            .Where(e =>
                e.AcademicExamGroupId == primaryExam.AcademicExamGroupId &&
                e.AcademicClassId == primaryExam.AcademicClassId &&
                e.AcademicSubjectId == primaryExam.AcademicSubjectId &&
                e.ExamCategory == primaryExam.ExamCategory &&
                e.Id != primaryExam.Id)
            .ToList();

        mergedExams.Insert(0, primaryExam);

        return mergedExams;
    }

    public Task<List<Student>> GetStudentsByExamIdAsync(int examId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RemoveByIdAsync(int id)
    {
        return await _academicExamRepository.RemoveByIdAsync(id);
    }
}

USE [SMSDB]
GO

/****** Object:  View [dbo].[vw_rpt_student_wise_marksheet]    Script Date: 01-Apr-24 6:58:53 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER           VIEW [dbo].[vw_rpt_student_wise_marksheet] 
AS
select DISTINCT
gr.ExamGroupName,
c.Name[ClassName],
s.Name[StudentName],
s.FatherName[FatherName],
s.MotherName,
s.ClassRoll,
sec.Name[SectionName],
sec.Id[AcademicSectionId],
g.Name[GenderName],
sub.SubjectName,
d.TotalMark,
d.ObtainMark,
d.GPA,
d.Grade,
(select MAX(sg_ERD.ObtainMark) from ExamResultDetails sg_ERD
	  inner join ExamResults sg_ER on sg_ERD.ExamResultId = sg_ER.Id
	  where sg_ERD.AcademicSubjectId = sub.Id and sg_ER.AcademicExamGroupId = gr.Id and sg_ER.AcademicClassId = c.Id)[MaxNumber],
r.CGPA[FinalGPA],
r.FinalGrade,
r.AttendancePercentage,
r.TotalObtainMarks,
r.TotalFails,
r.Rank[MeritPosition],
r.GradeComments,
gr.Id[ExamGroupId],
r.AcademicClassId,
r.StudentId,
s.DOB,
reli.Name[ReligionName],
r.CreatedAt
from ExamResults r
inner join ExamResultDetails d on r.Id=d.ExamResultId
inner join AcademicSubject sub on d.AcademicSubjectId = sub.Id
inner join Student s on r.StudentId=s.Id
inner join AcademicClass c on r.AcademicClassId = c.Id
left join AcademicSection sec on s.AcademicSectionId = sec.Id
left join Gender g on s.GenderId = g.Id
left join Religion reli on s.ReligionId = reli.Id
inner join AcademicExamGroups gr on r.AcademicExamGroupId = gr.Id
GO



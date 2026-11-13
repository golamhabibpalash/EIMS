USE [SMSDB]
GO

/****** Object:  View [dbo].[vw_rpt_Admit_Card_Info]    Script Date: 03/28/2026 11:41:56 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER   view [dbo].[vw_rpt_Admit_Card_Info] as
select s.Id[StudentId],
	s.ClassRoll,
	s.Name[StudentName],
	s.FatherName,
	s.MotherName,
	ses.Name[SessionName],
	c.Name[ClassName],
	sec.Id[AcademicSectionId],
	sec.Name[SectionName],
	sub.SubjectCode,
	sub.SubjectName,
	eg.ExamMonthId[MonthId],
	s.AcademicClassId,
	t.Id[ExamTypeId],
	t.ExamTypeName,
	r.Name[Religion],
	i.Name[InstituteName],
	i.EIIN,
	s.Status[StudentStauts],
	g.Name[Gender] from Institute i, (select exam.* from AcademicExams exam where exam.AcademicExamGroupId in (select gr.Id from AcademicExamGroups gr where gr.AcademicSessionid = (select sess.Id from AcademicSession sess where sess.CurrentSession = 1))) e
left join (select * from AcademicExamGroups where AcademicSessionId = (select Id from AcademicSession where CurrentSession = 1)) eg on e.AcademicExamGroupId = eg.Id
left join AcademicExamDetails d on e.Id = d.AcademicExamId
left join AcademicExamTypes t on eg.academicExamTypeId = t.Id
left join AcademicSubject sub on e.AcademicSubjectId = sub.Id
left join Student s on d.StudentId = s.Id
left join AcademicSession ses on s.AcademicSessionId = ses.Id
left join AcademicClass c on s.AcademicClassId = c.Id
left join AcademicSection sec on s.AcademicSectionId = sec.Id
left join Gender g on s.GenderId = g.Id
left join Religion r on s.ReligionId = r.Id
GO




-- Class Eight to Nine Science
DECLARE @NewAcademicClassId INT = 4;
DECLARE @OldAcademicClassId INT = 3;
DECLARE @OldAcademicSessionId INT = 4;
DECLARE @NewAcademicSessionId INT = 5;
DECLARE @NewRollPrefix VARCHAR(10)= '2509';
DECLARE @AcademicExamGroupId INT = 32;
update 
  s 
set 
  ClassRoll = 0 
from 
  student s 
where 
  s.AcademicClassId = @NewAcademicClassId 
  and s.AcademicSessionId = @NewAcademicSessionId;
update 
  s 
set 
  s.ClassRoll = @NewRollPrefix + RIGHT(
    '000' + CAST(r.Rank as varchar), 
    3
  ), 
  s.AcademicClassId = @NewAcademicClassId, 
  s.AcademicSessionId = @NewAcademicSessionId 
from 
  Student as s 
  join (
    select 
      t.* 
    from 
      ExamResults t 
    where 
      t.AcademicClassId = @OldAcademicClassId 
      and t.AcademicExamGroupId = @AcademicExamGroupId
  ) as r on s.Id = r.StudentId;
update 
  s 
set 
  s.Status = 0, 
  s.SMSService = 0 
from 
  Student s 
where 
  s.AcademicClassId = @OldAcademicClassId 
  and s.AcademicSessionId = @OldAcademicSessionId;
DECLARE @LastClassRoll int;
SELECT 
  @LastClassRoll = MAX(s.ClassRoll) 
from 
  Student s 
where 
  s.AcademicClassId = @NewAcademicClassId 
  and s.AcademicSessionId = @NewAcademicSessionId;
WITH OrderedStudents AS (
  SELECT 
    Id, 
    ROW_NUMBER() OVER (
      ORDER BY 
        Id
    ) AS RowNum 
  FROM 
    Student 
  WHERE 
    ClassRoll = 0
)
UPDATE 
  s 
SET 
  s.ClassRoll = CAST(@LastClassRoll + RowNum AS VARCHAR) 
FROM 
  Student AS s 
  JOIN OrderedStudents AS o ON s.Id = o.Id;


select 
  t.UniqueId, 
  t.ClassRoll, 
  t.* 
from 
  student t 
where 
  t.AcademicClassId = @NewAcademicClassId 
  and t.AcademicSessionId = @NewAcademicSessionId
order by 
  t.ClassRoll;

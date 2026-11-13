

CREATE OR ALTER PROCEDURE [dbo].[sp_GetAllStudentsDueSummary]
AS
BEGIN
    SET NOCOUNT ON;

    WITH StudentBaseData AS (
        SELECT 
            s.id AS StudentId,
            s.uniqueid AS UniqueId,
            s.AcademicSectionId,
            s.admissiondate AS AdmissionDate,
            s.academicclassid AS CurrentClassId,
            s.academicsessionid AS CurrentSessionId,
            s.isresidential AS ResidentialType
        FROM student s
    ),
    LastActivityData AS (
        SELECT 
            studentid,
            actiondatetime,
            ROW_NUMBER() OVER (PARTITION BY studentid ORDER BY id DESC) AS rn
        FROM studentactivatehists
        WHERE isactive = 0
    ),
    StudentCalculations AS (
        SELECT 
            sbd.StudentId,
            sbd.UniqueId,
            sbd.AcademicSectionId,
            sbd.AdmissionDate,
            sbd.CurrentClassId,
            sbd.CurrentSessionId,
            sbd.ResidentialType,
            COALESCE(lad.actiondatetime, GETDATE()) AS LastActivityDate,
            -- Calculate Starting Session
            CASE 
                WHEN YEAR(COALESCE(lad.actiondatetime, GETDATE())) = YEAR(sbd.AdmissionDate) 
                THEN sbd.CurrentSessionId
                ELSE sbd.CurrentSessionId - (YEAR(COALESCE(lad.actiondatetime, GETDATE())) - YEAR(sbd.AdmissionDate))
            END AS StartingSessionId
        FROM StudentBaseData sbd
        LEFT JOIN LastActivityData lad ON sbd.StudentId = lad.studentid AND lad.rn = 1
    ),
    StudentWithClasses AS (
        SELECT 
            *,
            CurrentClassId - (CurrentSessionId - StartingSessionId) AS StartingClassId
        FROM StudentCalculations
    ),
    PayableFees AS (
        SELECT 
            swc.StudentId,
            SUM(COALESCE(sfa.allocatedamount, cfl.amount)) AS PayableAmount
        FROM StudentWithClasses swc
        CROSS APPLY (
            SELECT cfl.id, cfl.amount, cfl.studentfeeheadid, cfl.AcademicSessionId, cfl.AcademicClassId
            FROM classfeelist cfl
            WHERE cfl.academicsessionid BETWEEN swc.StartingSessionId AND swc.CurrentSessionId
              AND cfl.AcademicClassId = (swc.StartingClassId + (cfl.AcademicSessionId - swc.StartingSessionId))
        ) cfl
        LEFT JOIN StudentFeeHead sfh
            ON cfl.studentfeeheadid = sfh.id
        LEFT JOIN studentfeeallocations sfa
            ON cfl.id = sfa.classfeelistid
            AND sfh.id = sfa.studentfeeheadid
            AND sfa.studentid = swc.StudentId
        WHERE sfh.isresidential = swc.ResidentialType
          AND (
              sfh.Id <> 2
              OR (
                  sfh.Id = 2
                  AND cfl.AcademicSessionId = swc.StartingSessionId
                  AND cfl.AcademicClassId = swc.StartingClassId
              )
          )
        GROUP BY swc.StudentId
    ),
    PaidFees AS (
        SELECT 
            sp.StudentId,
            SUM(sp.TotalPayment) AS PaidAmount
        FROM StudentPayment sp
        GROUP BY sp.StudentId
    )
    SELECT 
        swc.UniqueId,
        swc.StudentId,
        swc.AcademicSectionId,
        swc.CurrentClassId,
        ISNULL(pf.PayableAmount, 0) AS [PayableAmount],
        ISNULL(pd.PaidAmount, 0) AS [PaidAmount],
        ISNULL(pf.PayableAmount, 0) - ISNULL(pd.PaidAmount, 0) AS [DueAmount]
    FROM StudentWithClasses swc
    LEFT JOIN PayableFees pf ON swc.StudentId = pf.StudentId
    LEFT JOIN PaidFees pd ON swc.StudentId = pd.StudentId
    ORDER BY swc.StudentId;

END;



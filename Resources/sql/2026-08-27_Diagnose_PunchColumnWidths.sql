/* =====================================================================
   EIMS - Diagnostic only. Reads nothing but metadata, changes nothing.
   Safe to run any time. DBeaver: Execute script (Alt+X).

   Answers three questions before we touch anything:
     1. Is dbo.AttendanceMachines present, and is the T1 already registered?
     2. How wide are Tran_MachineRawPunch.MachineNo / CardNo really?
        MachineNo must hold 'WEF5261400145' = 13 chars.
     3. What default constraints / indexes depend on those columns?
        (These are what raise error 5074 on ALTER COLUMN.)
   ===================================================================== */

SET NOCOUNT ON;

/* 1. Does the machines table exist, and is the device registered? */
SELECT
    AttendanceMachines_Table = CASE WHEN OBJECT_ID('dbo.AttendanceMachines','U') IS NULL
                                    THEN 'MISSING' ELSE 'OK' END,
    T1_Registered            = CASE WHEN OBJECT_ID('dbo.AttendanceMachines','U') IS NULL THEN 'n/a'
                                    WHEN EXISTS (SELECT 1 FROM dbo.AttendanceMachines
                                                 WHERE SerialNumber = 'WEF5261400145')
                                    THEN 'YES' ELSE 'NO' END;

/* 2. Actual declared widths. max_length is BYTES; nvarchar = 2 bytes/char. */
SELECT
    ColumnName   = c.name,
    TypeName     = t.name,
    MaxChars     = CASE WHEN c.max_length = -1 THEN -1        -- -1 means (max)
                        WHEN t.name LIKE 'n%' THEN c.max_length / 2
                        ELSE c.max_length END,
    IsNullable   = c.is_nullable,
    NeedsAtLeast = CASE c.name WHEN 'MachineNo' THEN 13 ELSE NULL END,
    Verdict      = CASE
                     WHEN c.name <> 'MachineNo' THEN 'n/a'
                     WHEN c.max_length = -1 THEN 'OK (max)'
                     WHEN (CASE WHEN t.name LIKE 'n%' THEN c.max_length/2 ELSE c.max_length END) >= 13
                          THEN 'OK'
                     ELSE 'TOO NARROW - widening required'
                   END
FROM sys.columns c
JOIN sys.types   t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Tran_MachineRawPunch')
  AND c.name IN ('MachineNo','CardNo','MachineSerialNo','PayCode');

/* 3. Default constraints on those columns - the cause of error 5074.
      Note the auto-generated names differ per server, so never hardcode them. */
SELECT
    ConstraintName = dc.name,
    ColumnName     = c.name,
    Definition     = dc.definition
FROM sys.default_constraints dc
JOIN sys.columns c
  ON c.object_id = dc.parent_object_id
 AND c.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID('dbo.Tran_MachineRawPunch');

/* 4. Indexes touching those columns - these also block ALTER COLUMN. */
SELECT
    IndexName  = i.name,
    ColumnName = c.name,
    IsUnique   = i.is_unique
FROM sys.indexes i
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns c        ON c.object_id  = ic.object_id AND c.column_id = ic.column_id
WHERE i.object_id = OBJECT_ID('dbo.Tran_MachineRawPunch')
  AND c.name IN ('MachineNo','CardNo');

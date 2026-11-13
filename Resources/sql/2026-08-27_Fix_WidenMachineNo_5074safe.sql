/* =====================================================================
   EIMS - Widen Tran_MachineRawPunch.MachineNo to nvarchar(50), safely.

   WHY THE OLD SCRIPTS FAILED
   A plain "ALTER COLUMN MachineNo" raises error 5074 because an
   auto-named DEFAULT constraint (DF__Tran_Mach__Machi__<hex>) sits on
   the column. SQL Server refuses to change a column something depends
   on. The fix is drop-alter-recreate, looking the constraint up by
   catalog view rather than by name - the auto-generated suffix differs
   on every server, so a hardcoded name works nowhere but one machine.

   WHY IT IS NEEDED NOW
   The ADMS push path writes the device serial into MachineNo.
   'WEF5261400145' is 13 characters. If the column is narrower the
   inserts fail at runtime with a truncation error.

   >>> HOW TO RUN IN DBEAVER <<<
   Select the ENTIRE script, then press Ctrl+Enter (Execute SQL
   Statement). Do NOT use Alt+X here - Execute script splits on ';'
   and sends each piece as its own batch, which throws away the
   DECLARE'd variables. This one file is the exception to the
   one-statement-per-change convention used by the other scripts.

   Idempotent: re-running after success prints a message and stops.
   Wrapped in a transaction, so a mid-way failure rolls back cleanly.
   ===================================================================== */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @table        nvarchar(300) = N'dbo.Tran_MachineRawPunch';
DECLARE @col          sysname       = N'MachineNo';
DECLARE @targetChars  int           = 50;
DECLARE @conName      sysname;
DECLARE @conDef       nvarchar(max);
DECLARE @sql          nvarchar(max);
DECLARE @currentChars int;

SELECT @currentChars = CASE
                         WHEN c.max_length = -1      THEN 2147483647   -- nvarchar(max)
                         WHEN t.name LIKE 'n%'       THEN c.max_length / 2
                         ELSE c.max_length
                       END
FROM sys.columns c
JOIN sys.types   t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID(@table)
  AND c.name      = @col;

IF @currentChars IS NULL
BEGIN
    RAISERROR('Column %s was not found on %s - nothing done.', 16, 1, @col, @table);
    RETURN;
END

IF @currentChars >= @targetChars
BEGIN
    PRINT 'MachineNo is already ' + CAST(@currentChars AS varchar(20))
        + ' chars wide. No change needed.';
    RETURN;
END

PRINT 'Widening MachineNo from ' + CAST(@currentChars AS varchar(20))
    + ' to ' + CAST(@targetChars AS varchar(20)) + ' chars.';

BEGIN TRANSACTION;

    /* Capture the dependent default (name AND definition) before dropping it,
       so the column keeps the exact same default afterwards. */
    SELECT @conName = dc.name,
           @conDef  = dc.definition
    FROM sys.default_constraints dc
    JOIN sys.columns c
      ON c.object_id = dc.parent_object_id
     AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(@table)
      AND c.name = @col;

    IF @conName IS NOT NULL
    BEGIN
        PRINT 'Dropping dependent default constraint ' + @conName + ' ' + @conDef;
        SET @sql = N'ALTER TABLE ' + @table + N' DROP CONSTRAINT ' + QUOTENAME(@conName) + N';';
        EXEC sp_executesql @sql;
    END

    SET @sql = N'ALTER TABLE ' + @table
             + N' ALTER COLUMN ' + QUOTENAME(@col)
             + N' nvarchar(' + CAST(@targetChars AS nvarchar(10)) + N') NULL;';
    EXEC sp_executesql @sql;

    IF @conName IS NOT NULL
    BEGIN
        PRINT 'Recreating default constraint ' + @conName;
        SET @sql = N'ALTER TABLE ' + @table
                 + N' ADD CONSTRAINT ' + QUOTENAME(@conName)
                 + N' DEFAULT ' + @conDef
                 + N' FOR ' + QUOTENAME(@col) + N';';
        EXEC sp_executesql @sql;
    END

COMMIT TRANSACTION;

PRINT 'Done.';

/* Verify */
SELECT
    ColumnName = c.name,
    MaxChars   = CASE WHEN c.max_length = -1 THEN -1
                      WHEN t.name LIKE 'n%' THEN c.max_length / 2
                      ELSE c.max_length END,
    Verdict    = CASE WHEN c.max_length = -1
                       OR (CASE WHEN t.name LIKE 'n%' THEN c.max_length/2 ELSE c.max_length END) >= 13
                      THEN 'OK - fits WEF5261400145'
                      ELSE 'STILL TOO NARROW' END
FROM sys.columns c
JOIN sys.types   t ON t.user_type_id = c.user_type_id
WHERE c.object_id = OBJECT_ID('dbo.Tran_MachineRawPunch')
  AND c.name = 'MachineNo';

/* =====================================================================
   EIMS - Ticket module: navigation and permission rows.

   Run AFTER 2026-08-26_AddTicketModule_schema.sql.

   Creates the ProjectModules row the navigation gates on, its sub-module,
   and the seven ClaimStores rows so the permissions can be granted from
   Admin > Security > User Profile.

   No GO, no BEGIN/END, safe to re-run.
   ===================================================================== */

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

/* ---- 1. Module row. The navigation only renders a module whose SystemName
          exists here with Status = 1. ------------------------------------ */
IF NOT EXISTS (SELECT 1 FROM dbo.ProjectModules WHERE ModuleName = 'Ticket')
    INSERT INTO dbo.ProjectModules (ModuleName, Status, Remarks, CreatedAt, EditedAt)
    VALUES ('Ticket', 1, 'User tickets raised for the developer', GETDATE(), GETDATE());

/* ---- 2. Sub-module row ------------------------------------------------- */
IF NOT EXISTS (SELECT 1 FROM dbo.ProjectSubModules WHERE SubModuleName = 'TicketDesk')
    INSERT INTO dbo.ProjectSubModules (SubModuleName, ProjectModuleId, Status, Remarks, CreatedAt, EditedAt)
    SELECT 'TicketDesk', m.Id, 1, 'Ticket desk', GETDATE(), GETDATE()
    FROM dbo.ProjectModules m WHERE m.ModuleName = 'Ticket';

/* ---- 3. Claims. ClaimType is what the controller policy requires;
          ClaimValue is the policy name the menu matches on. ------------- */
INSERT INTO dbo.ClaimStores (ClaimType, ClaimValue, SubModuleId, CreatedAt, EditedAt)
SELECT c.ClaimType, c.ClaimValue, s.Id, GETDATE(), GETDATE()
FROM (VALUES
        ('View Tickets',          'IndexTicketsPolicy'),
        ('View Details Ticket',   'DetailsTicketsPolicy'),
        ('Create Ticket',         'CreateTicketsPolicy'),
        ('Edit Ticket',           'EditTicketsPolicy'),
        ('Delete Ticket',         'DeleteTicketsPolicy'),
        ('Change Ticket Status',  'ChangeStatusTicketsPolicy'),
        ('Comment On Ticket',     'CommentTicketsPolicy')
     ) AS c(ClaimType, ClaimValue)
CROSS JOIN dbo.ProjectSubModules s
WHERE s.SubModuleName = 'TicketDesk'
  AND NOT EXISTS (SELECT 1 FROM dbo.ClaimStores x WHERE x.ClaimValue = c.ClaimValue);

/* ---- 4. Verification --------------------------------------------------- */
SELECT
    Tickets_Table       = CASE WHEN OBJECT_ID('dbo.Tickets','U')            IS NULL THEN 'MISSING' ELSE 'OK' END,
    Comments_Table      = CASE WHEN OBJECT_ID('dbo.TicketComments','U')     IS NULL THEN 'MISSING' ELSE 'OK' END,
    Attachments_Table   = CASE WHEN OBJECT_ID('dbo.TicketAttachments','U')  IS NULL THEN 'MISSING' ELSE 'OK' END,
    Module_Row          = CASE WHEN EXISTS (SELECT 1 FROM dbo.ProjectModules    WHERE ModuleName = 'Ticket')     THEN 'OK' ELSE 'MISSING' END,
    SubModule_Row       = CASE WHEN EXISTS (SELECT 1 FROM dbo.ProjectSubModules WHERE SubModuleName = 'TicketDesk') THEN 'OK' ELSE 'MISSING' END,
    Claim_Rows          = (SELECT COUNT(*) FROM dbo.ClaimStores WHERE ClaimValue LIKE '%TicketsPolicy');

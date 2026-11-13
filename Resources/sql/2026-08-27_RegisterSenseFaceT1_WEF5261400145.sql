/* =====================================================================
   EIMS - Register the ZKTeco SenseFace T1 attendance terminal.

   Serial: WEF5261400145

   This device is Wi-Fi only and has no reachable listening port, so it
   talks to the server over the ADMS push protocol (/iclock/*) rather
   than the port 4370 pull path. IPAddress is therefore a placeholder -
   the terminal takes a DHCP lease that can change at any time, and the
   server never dials it. SerialNumber is the identity that matters:
   /iclock matches every incoming request on it, and refuses any serial
   that is not registered here.

   No GO, no BEGIN/END, one statement per change. Safe to re-run.
   Run with Execute script (Alt+X).
   ===================================================================== */

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM dbo.AttendanceMachines WHERE SerialNumber = 'WEF5261400145')
    INSERT INTO dbo.AttendanceMachines
        (Name, IPAddress, Port, SerialNumber, Model, Brand, Location,
         IsActive, UsePushMode, PushEndpoint, Username, Password,
         CreatedBy, CreatedAt, EditedBy, EditedAt)
    VALUES
        (N'SenseFace T1',      -- Name
         N'0.0.0.0',           -- IPAddress: unused in push mode, column is NOT NULL
         4370,                 -- Port: unused in push mode
         N'WEF5261400145',     -- SerialNumber: the device's identity on /iclock
         N'SenseFace T1',      -- Model
         1,                    -- Brand: MachineBrand.ZKTeco
         N'Main Gate',         -- Location: adjust to taste
         1,                    -- IsActive
         1,                    -- UsePushMode
         N'/iclock/cdata',     -- PushEndpoint: informational, the route is fixed in firmware
         N'admin',             -- Username: unused in push mode
         N'admin',             -- Password: unused in push mode
         N'setup-script', SYSDATETIME(), N'setup-script', SYSDATETIME());

SELECT Id, Name, SerialNumber, Model, Brand, IsActive, UsePushMode,
       PushEndpoint, LastSyncAt, LastError
FROM dbo.AttendanceMachines
WHERE SerialNumber = 'WEF5261400145';

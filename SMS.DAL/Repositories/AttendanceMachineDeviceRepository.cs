using Microsoft.EntityFrameworkCore;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories
{
    public class AttendanceMachineDeviceRepository : Repository<AttendanceMachine>, IAttendanceMachineDeviceRepository
    {
        public AttendanceMachineDeviceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<AttendanceMachine>> GetActiveMachinesAsync()
        {
            return await _context.AttendanceMachines
                .Where(m => m.IsActive)
                .ToListAsync();
        }

        public async Task<AttendanceMachine> GetBySerialNumberAsync(string serialNumber)
        {
            return await _context.AttendanceMachines
                .FirstOrDefaultAsync(m => m.SerialNumber == serialNumber);
        }
    }
}
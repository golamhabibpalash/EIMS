using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IAttendanceMachineDeviceRepository : IRepository<AttendanceMachine>
    {
        Task<IEnumerable<AttendanceMachine>> GetActiveMachinesAsync();
        Task<AttendanceMachine> GetBySerialNumberAsync(string serialNumber);
    }
}
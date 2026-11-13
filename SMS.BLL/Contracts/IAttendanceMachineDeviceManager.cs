using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IAttendanceMachineDeviceManager : IManager<AttendanceMachine>
    {
        Task<IEnumerable<AttendanceMachine>> GetActiveMachinesAsync();
        Task<AttendanceMachine> GetBySerialNumberAsync(string serialNumber);
        Task<bool> TestConnectionAsync(int machineId);
    }
}
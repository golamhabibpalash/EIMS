using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class AttendanceMachineDeviceManager : Manager<AttendanceMachine>, IAttendanceMachineDeviceManager
    {
        private readonly IAttendanceMachineDeviceRepository _attendanceMachineDeviceRepository;

        public AttendanceMachineDeviceManager(IAttendanceMachineDeviceRepository attendanceMachineDeviceRepository) 
            : base(attendanceMachineDeviceRepository)
        {
            _attendanceMachineDeviceRepository = attendanceMachineDeviceRepository;
        }

        public async Task<IEnumerable<AttendanceMachine>> GetActiveMachinesAsync()
        {
            return await _attendanceMachineDeviceRepository.GetActiveMachinesAsync();
        }

        public async Task<AttendanceMachine> GetBySerialNumberAsync(string serialNumber)
        {
            return await _attendanceMachineDeviceRepository.GetBySerialNumberAsync(serialNumber);
        }

        public async Task<bool> TestConnectionAsync(int machineId)
        {
            // This will be implemented in the service layer (ZKTecoService)
            // For now, just check if machine exists and is active
            var machine = await GetByIdAsync(machineId);
            return machine != null && machine.IsActive;
        }
    }
}
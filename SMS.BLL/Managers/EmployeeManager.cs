using BLL.Managers.Base;
using Repositories;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class EmployeeManager:Manager<Employee>, IEmployeeManager
    {
        private readonly IEmployeeRepository _employeeRepository;
        public EmployeeManager(IEmployeeRepository employeeRepository):base(employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Employee> GetByMachineUserIdAsync(string machineUserId)
        {
            return await _employeeRepository.GetByMachineUserIdAsync(machineUserId);
        }

        public async Task<Employee> GetByPhoneAttendance(string phoneLast9Digit)
        {
            return await _employeeRepository.GetByPhoneAttendance(phoneLast9Digit);
        }
    }
}

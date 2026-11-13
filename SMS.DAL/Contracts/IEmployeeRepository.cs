using SMS.DAL.Contracts.Base;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.DAL.Contracts
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<Employee> GetByPhoneAttendance(string phoneLast9Digit);

        /// <summary>
        /// Resolves the PIN an attendance terminal reports to the employee it
        /// was enrolled against. MachineUserId is the only field that carries
        /// that mapping - the phone-number match above is the legacy scheme.
        /// </summary>
        Task<Employee> GetByMachineUserIdAsync(string machineUserId);
    }
}

using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IEmployeeManager : IManager<Employee>
    {
        Task<Employee> GetByPhoneAttendance(string phoneLast9Digit);
        Task<Employee> GetByMachineUserIdAsync(string machineUserId);
    }
}

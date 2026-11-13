using BLL.Managers.Base;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.DAL.Repositories;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class AcademicSessionManager : Manager<AcademicSession>, IAcademicSessionManager
    {
        private readonly IAcademicSessionRepository _academicSessionRepository;
        public AcademicSessionManager(IAcademicSessionRepository academicSessionRepository):base(academicSessionRepository)
        {
            _academicSessionRepository = academicSessionRepository;
        }

        public async Task<AcademicSession> GetCurrentAcademicSessionAsync()
        {
            return await _academicSessionRepository.GetCurrentAcademicSession();
        }

        public Task<bool> IsExistByNameAsync(string name)
        {
           return _academicSessionRepository.Table.AnyAsync(s => s.Name .Equals(name));
        }
    }
}

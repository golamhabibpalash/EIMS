using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class AcademicSectionManager : Manager<AcademicSection>, IAcademicSectionManager
    {
        private readonly IAcademicSectionRepositoy _academicSectinRepository;
        public AcademicSectionManager(IAcademicSectionRepositoy academicSectionRepository):base(academicSectionRepository)
        {
            _academicSectinRepository = academicSectionRepository;
        }

        public async override Task<bool> AddAsync(AcademicSection entity)
        {
            var exist = await _academicSectinRepository.IsExistByNameWithClassNSessionAsync(entity);
            if (exist == true)
            {
                return false;
            }
            return await base.AddAsync(entity);
        }

        public async Task<IReadOnlyCollection<AcademicSection>> GetAllByClassWithSessionId(int classId, int sessionId)
        {
            return await _academicSectinRepository.GetAllByClassWithSessionId(classId, sessionId);
        }

        public async Task<IReadOnlyCollection<AcademicSection>> GetAllByExamGroupIdClassIdSessionId(int examGroupId, int classId, int sessionId)
        {
            return await _academicSectinRepository.GetAllByExamGroupId(examGroupId, classId, sessionId);
        }


        public async override Task<bool> UpdateAsync(AcademicSection entity)
        {
            var exitSection = await _academicSectinRepository.IsExistByNameWithClassNSessionAsync(entity);
            if (exitSection == true)
            {
                return false;
            }
            return await base.UpdateAsync(entity);
        }
    }
}

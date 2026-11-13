using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Managers.Base;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;

namespace SMS.BLL.Managers
{
    public class StudentFeeHeadManager : Manager<StudentFeeHead>, IStudentFeeHeadManager
    {
        private readonly IStudentFeeHeadRepository _studentFeeHeadRepository;
        private readonly IClassFeeListManager _classFeeListManager;

        public StudentFeeHeadManager(IStudentFeeHeadRepository studentFeeHeadRepository, IClassFeeListManager classFeeListManager) : base(studentFeeHeadRepository)
        {
            _studentFeeHeadRepository = studentFeeHeadRepository;
            _classFeeListManager = classFeeListManager;
        }

        public async Task<List<StudentFeeHead>> GetAllByClassIdSessionIdStudentIdAsync(int classId, int sessionId, int studentId)
        {
            var classFeeLists = await _classFeeListManager.GetByClassIdSessionIdStudentIdAsync(classId, sessionId, studentId);
            var studentFeeHeads = new List<StudentFeeHead>();
            foreach (var classFeeList in classFeeLists)
            {
                var studentFeeHead = await _studentFeeHeadRepository.GetByIdAsync(classFeeList.StudentFeeHeadId);
                if (studentFeeHead != null)
                {
                    studentFeeHeads.Add(studentFeeHead);
                }
            }
            return studentFeeHeads;
        }

        public async Task<StudentFeeHead> GetByNameAsync(string name)
        {
            return await _studentFeeHeadRepository.GetByNameAsync(name);
        }
    }
}

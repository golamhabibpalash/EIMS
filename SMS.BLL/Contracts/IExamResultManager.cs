using SMS.BLL.Contracts.Base;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts
{
    public interface IExamResultManager:IManager<ExamResult>
    {
        Task<List<ExamResult>> GetExamResultsByExamGroupNClassId(int examGroupId, int classId);
        bool IsResultProcessedAsync(int examGroupId, int classId);
        Task<Dictionary<(int ExamGroupId, int ClassId), bool>> GetResultProcessedStatusBulkAsync(IEnumerable<(int ExamGroupId, int ClassId)> examGroupAndClassPairs);
        Task<string> GetHighestMarksOfTheClassAsync(int examGroupId, int classId);
        Task<Dictionary<int, int>> GetPreviousRanksByStudentIdsAsync(int examGroupId, List<int> studentIds);
        Task<bool> DeleteByGroupAndClassAsync(int groupId, int classId);
    }
}

using SMS.BLL.Contracts.Base;
using SMS.Entities;
using SMS.Entities.AdditionalModels.StudentVM;
using System.Threading.Tasks;

namespace SMS.BLL.Contracts;

public interface IAttachDocManager : IManager<AttachDoc>
{
    Task<ProfileDocument> GetAllDocumentsByStudentId(int studentId);
}

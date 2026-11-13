using BLL.Managers.Base;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using SMS.Entities.AdditionalModels.StudentVM;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers;

public class AttachDocManager : Manager<AttachDoc>, IAttachDocManager
{
    private readonly IAttachDocRepository _attachDocRepository;
    public AttachDocManager( IAttachDocRepository attachDocRepository) : base(attachDocRepository)
    {
        _attachDocRepository = attachDocRepository;
    }

    public async Task<ProfileDocument> GetAllDocumentsByStudentId(int studentId)
    {
        ProfileDocument profileDocument = new ProfileDocument();
        var allDocuments = await _attachDocRepository.Table.Where(s => s.StudentId == studentId).ToListAsync();

        if (allDocuments != null && allDocuments.Count() > 0)
        {
            DocInfo docInfo = new DocInfo()
            {

            };
            profileDocument.Documents.Add(docInfo);
        }

        return profileDocument;
    }
}

using SMS.DAL.Contracts;
using SMS.DAL.Repositories.Base;
using SMS.DB;
using SMS.Entities;

namespace SMS.DAL.Repositories;

public class AttachDocRepository : Repository<AttachDoc>, IAttachDocRepository
{
    public AttachDocRepository(ApplicationDbContext context) : base(context)
    {
    }
}

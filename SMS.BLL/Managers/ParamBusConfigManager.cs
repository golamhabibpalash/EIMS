using BLL.Managers.Base;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers
{
    public class ParamBusConfigManager : Manager<ParamBusConfig>, IParamBusConfigManager
    {
        private readonly IParamBusConfigRepository _paramRepository;

        public ParamBusConfigManager(IParamBusConfigRepository repository) : base(repository)
        {
            _paramRepository = repository;
        }

        public async Task<ParamBusConfig> GetByParamSL(int paramSL)
        {
            return await _paramRepository.Table.FirstOrDefaultAsync(s => s.ParamSL == paramSL);
        }

        public async Task<string> GetValueByParamSL(int paramSL)
        {
            return await _paramRepository.Table
                .Where(s => s.ParamSL ==paramSL)
                .Select(s => s.ParamValue)
                .FirstOrDefaultAsync();
        }
    }
}

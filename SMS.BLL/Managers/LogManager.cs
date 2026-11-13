using System;
using System.Threading.Tasks;
using SMS.BLL.Contracts;
using SMS.Entities;
using BLL.Managers.Base;
using SMS.DAL.Contracts;

namespace SMS.BLL.Managers;

public class LogManager :Manager<Log>, ILogManager
{
    private readonly ILogRepository _logRepository;
    public LogManager(ILogRepository logRepository) : base(logRepository)
    {
        _logRepository = logRepository;
    }
}

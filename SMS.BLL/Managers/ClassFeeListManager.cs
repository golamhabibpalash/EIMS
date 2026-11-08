using BLL.Managers.Base;
using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts;
using SMS.DAL.Contracts;
using SMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SMS.BLL.Managers;

public class ClassFeeListManager : Manager<ClassFeeList>, IClassFeeListManager
{
    private readonly IClassFeeListRepository _classFeeListRepository;
    private readonly IStudentManager _studentManager;
    private readonly IAcademicSessionManager _sessionManager;
    private readonly IStudentFeeAllocationManager _studentFeeAllocationManager;

    public ClassFeeListManager(IClassFeeListRepository classFeeListRepository, IStudentManager studentManager, IAcademicSessionManager sessionManager, IStudentFeeAllocationManager studentFeeAllocationManager) : base(classFeeListRepository)
    {
        _classFeeListRepository = classFeeListRepository;
        _studentManager = studentManager;
        _sessionManager = sessionManager;
        _studentFeeAllocationManager = studentFeeAllocationManager;
    }

    public async Task<List<ClassFeeList>> GetAllByClassIdAsync(int classId)
    {
        return await _classFeeListRepository.GetAllByClassIdAsync(classId);
    }

    public async Task<List<ClassFeeList>> GetAllByStudentId(int studId)
    {
        Student student = await _studentManager.GetByIdAsync(studId);
        List<ClassFeeList> classFeeLists =await _classFeeListRepository.Table.Where(s => s.AcademicClassId == student.AcademicClassId && s.AcademicSessionId == student.AcademicSessionId && s.StudentFeeHead.IsResidential == student.IsResidential).ToListAsync();
        return classFeeLists;
    }

    public async Task<ClassFeeList> GetByClassIdAndFeeHeadIdAsync(int classId, int feeHeadId, int sessionId)
    {
        var result = await _classFeeListRepository.GetByClassIdAndFeeHeadIdAsync(classId, feeHeadId,sessionId);
        return result;
    }

    public async Task<List<ClassFeeList>> GetByClassIdSessionIdStudentIdAsync(int classId, int sessionId, int studentId)
    {
        var allClassFeeLists = await _classFeeListRepository.GetByClassIdSessionIdStudentIdAsync(classId, sessionId, studentId);

        // find and set all allocations
        var studentAllAllocations =  await _studentFeeAllocationManager.GetStudentFeeAllocationsByStudentId(studentId);

        if (studentAllAllocations != null && studentAllAllocations.Count > 0)
        {
            var activeAllocations = studentAllAllocations
                .Where(s => s.StudentId == studentId && s.IsActive)
                .ToList();

            foreach (var cfl in allClassFeeLists)
            {
                // Prefer the allocation tied to this exact class fee (head + session); fall back to
                // legacy allocations saved without a ClassFeeListId, matched by fee head.
                var allocation = activeAllocations.FirstOrDefault(a => a.ClassFeeListId == cfl.Id)
                    ?? activeAllocations.FirstOrDefault(a => a.ClassFeeListId == null && a.StudentFeeHeadId == cfl.StudentFeeHeadId);

                // A zero allocation is valid: it means the head is free for this student.
                if (allocation != null)
                {
                    cfl.Amount = allocation.AllocatedAmount;
                }
            }
        }
        return allClassFeeLists;
    }

    public async Task<List<ClassFeeList>> GetClassFeeListByClassIdFeeHeadIdSessionIdAsync(int classId, int feeHeadId, int sessionId)
    {
        List<ClassFeeList> result = await _classFeeListRepository.GetClassFeeListByClassIdFeeHeadIdSessionIdAsync(classId, feeHeadId, sessionId);
        return result;
    }
    public async Task<double> GetFeeAmountByFeeListSlAsync(string uniquId, int sl)
    {
        var result = await _classFeeListRepository.GetFeeAmountByFeeListSL(uniquId, sl);
        return result;
    }
    public async Task<List<ClassFeeList>> GetClassFeeByUniquId(string uniquId)
    {
        var result = await _classFeeListRepository.GetCurrentAllByUniqueId(uniquId);
        return result;
    }

    public async Task<List<ClassFeeList>> GetAllBySessionClassTypeAsync(int sessionId, int classId, bool isResidential)
    {
        var allListByClass = await _classFeeListRepository.GetAllBySessionIdClassIdAsync(sessionId, classId);
        var result = allListByClass.Where(s => s.StudentFeeHead.IsResidential == isResidential).ToList();
        
        return result;
    }    
}

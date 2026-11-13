using Microsoft.EntityFrameworkCore;
using SMS.BLL.Contracts.Base;
using SMS.DAL.Contracts.Base;
using SMS.DAL.Repositories.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BLL.Managers.Base;

public abstract class Manager<T> : IManager<T> where T : class
{
    protected readonly SMS.DAL.Contracts.Base.IRepository<T> _repository;
    public Manager(SMS.DAL.Contracts.Base.IRepository<T> repository)
    {
        _repository = repository;
    }

    public virtual async Task<bool> AddAsync(T entity)
    {
        return await _repository.AddAsync(entity);
    }

    public virtual async Task<bool> AddRangeAsync(IReadOnlyCollection<T> entities)
    {
        foreach (var entity in entities)
        {
            await _repository.AddAsync(entity);
        }
        return await _repository.SaveAfterAddAsync();
    }

    public virtual async Task<IReadOnlyCollection<T>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public virtual async Task<bool> RemoveAsync(T entity)
    {
        return await _repository.RemoveAsync(entity);
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        return await _repository.UpdateAsync(entity);
    }

    public virtual async Task<bool> IsExistByIdAsync(int id)
    {
        return await _repository.IsExistByIdAsync(id);
    }

    public virtual async Task<bool> IsExistAsync(T entity)
    {
        return await _repository.IsExistAsync(entity);
    }

    public T GetById(int id)
    {
        return _repository.GetById(id);
    }

    public async Task<bool> SaveAfterAddAsync()
    {
        return await _repository.SaveAfterAddAsync();
    }
    public bool AddWithoutSave(T entity)
    {
        return _repository.AddWithoutSave(entity);
    }
    public async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _repository.FindAsync(predicate);
    }

    public async Task<List<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
    {
        return await _repository.FindAllAsync(predicate);
    }
}

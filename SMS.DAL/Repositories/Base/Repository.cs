using Microsoft.EntityFrameworkCore;
using SMS.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SMS.DAL.Repositories.Base
{
    public abstract class Repository<T> : Contracts.Base.IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        //private DbSet<T> _entities;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }
        public DbSet<T> Entity
        {
            get { return _context.Set<T>(); }
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            return await Entity.FindAsync(id);
        }
        public virtual async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await Entity.ToListAsync();
        }
        public virtual async Task<bool> AddAsync(T entity)
        {
            try
            {
                await Entity.AddAsync(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddAsync: {ex.Message}");
                return false;
            }
        }
        public virtual async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateAsync: {ex.Message}");
                return false;
            }
        }
        public virtual async Task<bool> RemoveAsync(T entity)
        {
            try
            {
                Entity.Remove(entity);
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddAsync: {ex.Message}");
                return false;
            }
        }
        public virtual async Task<bool> IsExistByIdAsync(int id)
        {
            var result = await Entity.FindAsync(id);
            if (result != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public virtual async Task<bool> IsExistAsync(T entity)
        {
            await Entity.FindAsync(entity);
            return true;
        }
        public T GetById(int id)
        {
            return Entity.Find(id);
        }
        public async Task<bool> SaveAfterAddAsync()
        {
            try
            {
                bool isSaved = await _context.SaveChangesAsync() > 0;
                if (isSaved)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
        public bool AddWithoutSave(T entity)
        {
            try
            {
                Entity.Add(entity);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public virtual IQueryable<T> Table => Entity;
        public virtual async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await Entity.FirstOrDefaultAsync(predicate);
        }
        public virtual async Task<List<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await Entity.Where(predicate).ToListAsync();
        }
    }
}

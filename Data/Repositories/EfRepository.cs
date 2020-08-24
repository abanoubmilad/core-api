using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using core_api.Models.Pagination;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace core_api.Data
{
    public class EfRepository
    {

        protected readonly DataContext _context;

        public EfRepository(DataContext context)
        {
            _context = context;
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return _context.Set<T>();
        }


        public virtual async Task<T> FindByIdAsync<T>(object id) where T : class
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T> FirstOrDefaulBySpecAsync<T>(ISpecification<T> spec) where T : class
        {
            return await Query(spec)
                   .FirstOrDefaultAsync();
        }

        public async Task<List<T>> ListAsync<T>(ISpecification<T> spec) where T : class
        {
            return await Query(spec)
                            .ToListAsync();
        }

        public IQueryable<T> ApplyPageQuery<T>(IQueryable<T> query, PageQuery pageQery)
        {
            return query.Skip((pageQery.PageNumber - 1) * pageQery.PageSize)
                           .Take(pageQery.PageSize);
        }

        public async Task<List<T>> ListPagiantionAsync<T>(IQueryable<T> query, PageQuery pageQery)
        {
            return await ApplyPageQuery(query, pageQery).ToListAsync();
        }

        public async Task<List<T>> ListAsync<T>(IQueryable<T> query)
        {
            return await query.ToListAsync();
        }

        public async Task<int> CountAsync<T>(IQueryable<T> query)
        {
            return await query.CountAsync();
        }

        public IQueryable<T> Query<T>(ISpecification<T> spec) where T : class
        {
            // fetch a Queryable that includes all expression-based includes
            var queryResultWithIncludes = spec.Includes
                .Aggregate(_context.Set<T>().AsQueryable(),
                    (current, include) => current.Include(include));

            // modify the IQueryable to include any string-based include statements
            var secondaryResult = spec.IncludeStrings
                .Aggregate(queryResultWithIncludes,
                    (current, include) => current.Include(include));

            // return the result of the query using the specification's criteria expression
            return secondaryResult
                            .Where(spec.Criteria);
        }

        public EntityEntry<T> Add<T>(T entity) where T : class
        {
            return _context.Set<T>().Add(entity);
        }

        public async Task<T> AddAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public Task<int> CommitAsnyc()
        {
            return _context.SaveChangesAsync();
        }

        public int Commit()
        {
            return _context.SaveChanges();
        }


        public EntityEntry<T> Update<T>(T entity) where T : class
        {
            return _context.Set<T>().Update(entity);
        }

        public async Task<int> UpdateAsync<T>(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public EntityEntry<T> Delete<T>(T entity) where T : class
        {
            return _context.Set<T>().Remove(entity);
        }

        public async Task<int> DeleteAsync<T>(T entity) where T : class
        {
            _context.Set<T>().Remove(entity);
            return await _context.SaveChangesAsync();
        }

        // todo should use this and check IsolationLevels
        public IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Snapshot)
        {
            return _context.Database.BeginTransaction(isolationLevel);
        }
    }
}
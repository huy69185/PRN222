using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ClassroomBooking.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public async Task DeleteAsync(T entity) => _dbSet.Remove(entity);
        public void Update(T entity) => _dbSet.Update(entity);

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;
            if (filter != null) query = query.Where(filter);
            foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(includeProp.Trim());
            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(object id) => await _dbSet.FindAsync(id);

        public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string includeProperties = "")
        {
            IQueryable<T> query = _dbSet.Where(filter);
            foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(includeProp.Trim());
            return await query.FirstOrDefaultAsync();
        }
    }
}
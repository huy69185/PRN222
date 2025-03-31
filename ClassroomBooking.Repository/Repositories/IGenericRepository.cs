using System.Linq.Expressions;

namespace ClassroomBooking.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        Task AddAsync(T entity);
        Task DeleteAsync(T entity);
        void Update(T entity);
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string includeProperties = "");
        Task<T?> GetByIdAsync(object id);
        Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> filter, string includeProperties = "");
    }
}
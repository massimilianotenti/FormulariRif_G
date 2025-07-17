// File: Data / IGenericRepository.cs
// Interfaccia per un repository generico.
// Definisce i metodi CRUD comuni che possono essere implementati per qualsiasi entità.
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using FormulariRif_G.Data; // <--- ASSICURATI CHE QUESTO SIA PRESENTE per AppDbContext

namespace FormulariRif_G.Data
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
        IQueryable<T> AsQueryable();
        AppDbContext GetContext();
    }
}
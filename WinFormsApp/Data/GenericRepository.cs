// File: Data/GenericRepository.cs
// Implementazione del repository generico.
// Fornisce i metodi CRUD per interagire con il database tramite DbContext.
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Collections.Generic; // <--- AGGIUNGI QUESTA RIGA
using System.Threading.Tasks;     // <--- AGGIUNGI QUESTA RIGA
using FormulariRif_G.Data;

namespace FormulariRif_G.Data
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>(); // Ottiene il DbSet per il tipo T
        }

        /// <summary>
        /// Recupera un'entità per ID.
        /// </summary>
        /// <param name="id">L'ID dell'entità.</param>
        /// <returns>L'entità trovata o null se non esiste.</returns>
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Recupera tutte le entità di un tipo specifico.
        /// </summary>
        /// <returns>Una collezione di tutte le entità.</returns>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Trova entità che corrispondono a un predicato specificato.
        /// </summary>
        /// <param name="predicate">L'espressione di filtro.</param>
        /// <returns>Una collezione di entità che corrispondono al predicato.</returns>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Aggiunge una nuova entità al contesto.
        /// </summary>
        /// <param name="entity">L'entità da aggiungere.</param>
        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        /// <summary>
        /// Marca un'entità come modificata nel contesto.
        /// Se l'entità è già tracciata, aggiorna i suoi valori.
        /// </summary>
        /// <param name="entity">L'entità da aggiornare.</param>
        public void Update(T entity)
        {
            // Ottiene l'Entry per l'entità. Questo non la attacca se non è già tracciata.
            var entry = _context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                // Se l'entità non è tracciata, la attacca e la marca come modificata.
                _dbSet.Attach(entity);
                entry.State = EntityState.Modified;
            }
            else if (entry.State == EntityState.Added)
            {
                // Se l'entità è già in stato "Added" (nuovo record non ancora salvato),
                // non è necessario fare nulla. Verrà salvata correttamente da SaveChangesAsync.
                // Tentare di marcarla come Modified causerebbe l'errore di ID temporaneo.
            }
            else
            {
                // Se l'entità è già tracciata (es. Unchanged o Modified),
                // si assicura che il suo stato sia Modified per salvare le modifiche.
                entry.State = EntityState.Modified;
            }
        }

        /// <summary>
        /// Marca un'entità come eliminata nel contesto.
        /// </summary>
        /// <param name="entity">L'entità da eliminare.</param>
        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        /// <summary>
        /// Salva tutte le modifiche pendenti nel database.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public IQueryable<T> AsQueryable()
        {
            return _dbSet.AsQueryable();
        }

        /// <summary>
        /// Restituisce l'istanza del DbContext associata a questo repository.
        /// </summary>
        /// <returns>Il DbContext.</returns>
        public AppDbContext GetContext()
        {
            return _context;
        }
    }
}
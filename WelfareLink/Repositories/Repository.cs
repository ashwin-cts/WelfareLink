using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
using WelfareLink.Interfaces;

namespace WelfareLink.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly WelfareLinkDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(WelfareLinkDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            // Check if the entity is already being tracked
            var local = _context.Set<T>()
                .Local
                .FirstOrDefault(e => EF.Property<int>(e, GetKeyPropertyName()) == GetEntityKey(entity));

            if (local != null)
            {
                // Detach the local entity if it exists
                _context.Entry(local).State = EntityState.Detached;
            }

            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        private string GetKeyPropertyName()
        {
            // Try common key property names
            var entityType = typeof(T);
            var properties = entityType.GetProperties();

            // Check for common key patterns
            if (properties.Any(p => p.Name == $"{entityType.Name}ID"))
                return $"{entityType.Name}ID";
            if (properties.Any(p => p.Name == "Id"))
                return "Id";
            if (properties.Any(p => p.Name == "ID"))
                return "ID";

            // Fallback to first property ending with ID
            var idProp = properties.FirstOrDefault(p => p.Name.EndsWith("ID", StringComparison.OrdinalIgnoreCase));
            return idProp?.Name ?? "Id";
        }

        private int GetEntityKey(T entity)
        {
            var keyPropertyName = GetKeyPropertyName();
            var property = typeof(T).GetProperty(keyPropertyName);
            if (property != null)
            {
                var value = property.GetValue(entity);
                if (value is int intValue)
                    return intValue;
            }
            return 0;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.AnyAsync(e => EF.Property<int>(e, "ApplicationID") == id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using teladoc.domain.Contracts.Repositories;

namespace teladoc.dao
{
    public class Repository<TEntity> : IRepository<TEntity>
      where TEntity : class
    {
        protected readonly DbContext _db;
        protected readonly DbSet<TEntity> _set;

        public Repository(DbContext db)
        {
            _db = db;
            _set = _db.Set<TEntity>();
        }

        public async Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default)
        {
            return await _set.FindAsync([id], ct);
        }

        public async Task AddAsync(TEntity entity, CancellationToken ct = default)
        {
            await _set.AddAsync(entity, ct);
        }

        public void Update(TEntity entity)
        {
            _set.Update(entity);
        }

        public void Remove(TEntity entity)
        {
            _set.Remove(entity);
        }
    }
}

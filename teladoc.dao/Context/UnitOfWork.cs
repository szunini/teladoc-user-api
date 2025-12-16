using teladoc.domain.Contracts.Services;

namespace teladoc.dao.Context
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly TeladocDbContext _db;

        public UnitOfWork(TeladocDbContext db)
        {
            _db = db;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return _db.SaveChangesAsync(ct);
        }             
    }
}

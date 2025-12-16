using Microsoft.EntityFrameworkCore;
using teladoc.dao.Context;
using teladoc.domain.Contracts.Repositories;
using teladoc.domain.Entities;

namespace teladoc.dao.repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(TeladocDbContext db) : base(db) { }

        public async Task<IEnumerable<User>> GetAll(CancellationToken cancellationToken)
        {
            return await _set.AsNoTracking().ToListAsync(cancellationToken);
        }

        public async Task<User> GetByEmail(string email, CancellationToken cancellationToken)
        {
            var user = await _set
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

            if (user == null)
            {
                throw new InvalidOperationException($"There is no user with email: '{email}'.");
            }

            return user;
        }
        
        public async Task<bool> ExistsEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _set
                .AsNoTracking()
                .AnyAsync(u => u.Email == email, cancellationToken);
        }

        public Task<User?> GetByIdForUpdateAsync(int id, CancellationToken cancellationToken = default)
        {            
            return _set.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }
    }
}

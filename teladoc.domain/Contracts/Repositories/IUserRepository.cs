using teladoc.domain.Entities;

namespace teladoc.domain.Contracts.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Get All users
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<User>> GetAll(CancellationToken cancellationToken);
        
        /// <summary>
        /// Get User By email 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<User> GetByEmail(string email, CancellationToken cancellationToken);

        
        /// <summary>
        /// Check if the Email  is already used
        /// </summary>
        /// <param name="email"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ExistsEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get id without tracking
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<User?> GetByIdForUpdateAsync(int id, CancellationToken ct = default);
    }
}


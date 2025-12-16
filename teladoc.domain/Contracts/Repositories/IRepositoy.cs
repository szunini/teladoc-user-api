namespace teladoc.domain.Contracts.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Get Entity by Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default);

        /// <summary>
        /// Add Entity by Id
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task AddAsync(TEntity entity, CancellationToken ct = default);
        
        /// <summary>
        /// Update Entity 
        /// </summary>
        /// <param name="entity"></param>
        void Update(TEntity entity);
        
        /// <summary>
        /// Remove Entity
        /// </summary>
        /// <param name="entity"></param>
        void Remove(TEntity entity);

    }
}

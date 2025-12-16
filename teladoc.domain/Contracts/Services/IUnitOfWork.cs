using System;
using System.Collections.Generic;
using System.Text;

namespace teladoc.domain.Contracts.Services
{
    public interface IUnitOfWork
    {
     
        /// <summary>
        /// Save changes in the database
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}

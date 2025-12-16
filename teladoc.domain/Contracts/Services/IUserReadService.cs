using System;
using System.Collections.Generic;
using System.Text;
using teladoc.domain.DTOs;

namespace teladoc.domain.Contracts.Services
{
    public interface IUserReadService : IUserCacheInvalidator
    {
        /// <summary>
        /// Get an element, first check the cache then go to the database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<UserResponse> GetByIdAsync(int id, CancellationToken ct = default);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace teladoc.domain.Contracts.Services
{
    public interface IUserCacheInvalidator
    {
        /// <summary>
        /// Remove the element from the cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task InvalidateByIdAsync(int id);
    }

}

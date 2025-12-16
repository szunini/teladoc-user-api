using Microsoft.AspNetCore.JsonPatch;
using System.Threading.Tasks;
using teladoc.domain.DTOs;
using teladoc.domain.Entities;
using teladoc.domain.Enum;

namespace teladoc.domain.Contracts.Services
{
    /// <summary>
    ///User Service
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get user by identifier.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<UserResponse> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="request">user data</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all users.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Update partially the user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PatchUserResult> UpdateUserAsync(int id, UpdateUserCommand command, CancellationToken ct);

        /// <summary>
        /// Update totally the user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PatchUserResult> ReplaceUserAsync(int id, UpdateUserRequest request, CancellationToken ct = default);

        /// <summary>
        /// Delete the user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<DeleteUserResultEnum> DeleteUserAsync(int id, CancellationToken ct = default);
    }
}

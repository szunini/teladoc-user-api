using teladoc.domain.Contracts.Repositories;
using teladoc.domain.Contracts.Services;
using teladoc.domain.DTOs;
using teladoc.domain.Entities;
using teladoc.domain.Enum;
using teladoc.domain.Exceptions;

namespace teladoc.domain.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _uow;
        private readonly TimeProvider _timeProvider;
        private readonly IUserReadService _readService;


        public UserService(IUserRepository repo, IUnitOfWork uow, TimeProvider timeProvider, IUserReadService readService)
        {
            _userRepository = repo;
            _uow = uow;
            _timeProvider = timeProvider;
            _readService = readService;
        }


        public async Task<UserResponse> GetUserByIdAsync(int id, CancellationToken ct = default)
        {
            return  await _readService.GetByIdAsync(id, ct);
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.GetAll(cancellationToken);
            return users.Select(MapToUserResponse);
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
        {
            var age = CalculateAge(request.DateOfBirth);
            if (age < 18)
            {
                throw new UserValidationException("DateOfBirth", "User must be at least 18 years old.");
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                NickName = request.NickName,
                FriendCount = request.FriendCount ?? 0
            };

            user.SetEmail(request.Email);

            await _userRepository.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);
            await _readService.InvalidateByIdAsync(user.Id);

            return MapToUserResponse(user);
        }

        public async Task<PatchUserResult> UpdateUserAsync(int id, UpdateUserCommand cmd, CancellationToken ct)
        {
            var user = await _userRepository.GetByIdForUpdateAsync(id, ct);
            if (user is null)
                return new PatchUserResult(PatchUserResultEnum.NotFound);

            var errors = new Dictionary<string, string[]>();

            if (cmd.FirstName is not null && string.IsNullOrWhiteSpace(cmd.FirstName))
                errors["FirstName"] = new[] { "FirstName cannot be empty." };

            if (cmd.LastName is not null && string.IsNullOrWhiteSpace(cmd.LastName))
                errors["LastName"] = new[] { "LastName cannot be empty." };

            if (cmd.DateOfBirth.HasValue && CalculateAge(cmd.DateOfBirth.Value) < 18)
                errors["DateOfBirth"] = new[] { "User must be at least 18 years old." };

            if (errors.Count > 0)
            {
                return new PatchUserResult(PatchUserResultEnum.ValidationError, errors);
            }

            if (cmd.FirstName is not null) user.FirstName = cmd.FirstName;
            if (cmd.LastName is not null) user.LastName = cmd.LastName;
            if (cmd.Nickname is not null) user.NickName = cmd.Nickname;
            if (cmd.FriendCount.HasValue) user.FriendCount = cmd.FriendCount.Value;
            if (cmd.DateOfBirth.HasValue) user.DateOfBirth = cmd.DateOfBirth.Value;
            if (cmd.Email is not null) user.SetEmail(cmd.Email);

            await _uow.SaveChangesAsync(ct);
            await _readService.InvalidateByIdAsync(user.Id);
            return new PatchUserResult(PatchUserResultEnum.Ok);
        }

        public async Task<PatchUserResult> ReplaceUserAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdForUpdateAsync(id, ct);
            if (user is null)
                return new PatchUserResult(PatchUserResultEnum.NotFound);

            var age = CalculateAge(request.DateOfBirth);
            if (age < 18)
            {
                return new PatchUserResult(
                    PatchUserResultEnum.ValidationError,
                    new Dictionary<string, string[]>
                    {
                        ["DateOfBirth"] = new[] { "User must be at least 18 years old." }
                    });
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.DateOfBirth = request.DateOfBirth;
            user.NickName = request.NickName;
            user.FriendCount = request.FriendCount ?? user.FriendCount;
            user.SetEmail(request.Email);

            await _uow.SaveChangesAsync(ct);
            await _readService.InvalidateByIdAsync(user.Id);
            return new PatchUserResult(PatchUserResultEnum.Ok);
        }



        private UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                NickName = user.NickName,
                FriendCount = user.FriendCount,
                Age = CalculateAge(user.DateOfBirth)
            };
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var today = _timeProvider.GetUtcNow().DateTime.Date;

            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }

        public async Task<DeleteUserResultEnum> DeleteUserAsync(int id, CancellationToken ct = default)
        {
            // Traer TRACKED
            var user = await _userRepository.GetByIdForUpdateAsync(id, ct);
            if (user is null)
                return DeleteUserResultEnum.NotFound;

            _userRepository.Remove(user);
            await _uow.SaveChangesAsync(ct);
            await _readService.InvalidateByIdAsync(user.Id);
            return DeleteUserResultEnum.Ok;
        }


    }
}

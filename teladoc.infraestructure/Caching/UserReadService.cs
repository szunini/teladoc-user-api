using StackExchange.Redis;
using System.Text.Json;
using teladoc.domain.Contracts.Repositories;
using teladoc.domain.Contracts.Services;
using teladoc.domain.DTOs;
using teladoc.domain.Entities;
using teladoc.domain.Exceptions;

namespace teladoc.infraestructure.Caching;

public sealed class UserReadService : IUserReadService
{
    private readonly IUserRepository _repo;
    private readonly StackExchange.Redis.IDatabase _db;
    private readonly TimeProvider _timeProvider;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public UserReadService(IUserRepository repo, IConnectionMultiplexer mux, TimeProvider timeProvider)
    {
        _repo = repo;
        _db = mux.GetDatabase();
        _timeProvider = timeProvider;
    }

    private static string KeyById(int id)
    {
        return $"users:v1:response:id:{id}";
    }

    public async Task<UserResponse> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var key = KeyById(id);

        var cached = await _db.StringGetAsync(key);
        if (cached.HasValue)
        {
            var dto = JsonSerializer.Deserialize<UserResponse>(
                cached.ToString(),
                JsonOpts);

            if (dto is not null)
                return dto;
        }

        var user = await _repo.GetByIdAsync(id, ct);
        if (user is null)
        {
            throw new UserNotFoundException(id);
        }

        var dtoFresh = MapToUserResponse(user);

        // TTL + jitter
        var ttl = TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(Random.Shared.Next(0, 60));
        await _db.StringSetAsync(key, JsonSerializer.Serialize(dtoFresh, JsonOpts), ttl);

        return dtoFresh;
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
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }

    //delete the user id from cache
    public async Task InvalidateByIdAsync(int id)
    {
        await _db.KeyDeleteAsync(KeyById(id));
    }
}

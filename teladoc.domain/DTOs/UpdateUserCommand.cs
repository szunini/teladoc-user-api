namespace teladoc.domain.DTOs
{
    /// <summary>
    /// DTO to update partially a user
    /// </summary>
    public class UpdateUserCommand
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Nickname { get; init; }
        public int? FriendCount { get; init; }

        public string? Email { get; init; }

        public DateTime? DateOfBirth { get; init; }
    }
}

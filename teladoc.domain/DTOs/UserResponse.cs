namespace teladoc.domain.DTOs
{
    /// <summary>
    /// DTO to reuturn user info.
    /// </summary>
    public class UserResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? NickName { get; set; }
        public int? FriendCount { get; set; }
        public int Age { get; set; }
    }
}

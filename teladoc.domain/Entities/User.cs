namespace teladoc.domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; private set; } = null!;
        public string EmailNormalized { get; private set; } = null!;


        public DateTime DateOfBirth { get; set; }

        public string? NickName { get; set; }

        public int? FriendCount { get; set; }
                
        public void SetEmail(string email)
        {
            Email = email;
            EmailNormalized = email.Trim().ToUpperInvariant();
        }
    }
}

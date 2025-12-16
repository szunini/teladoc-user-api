namespace teladoc.domain.Exceptions
{
    public class UserNotFoundException : Exception
    {
        public int UserId { get; }

        public UserNotFoundException(int userId)
            : base($"User Id {userId} not found.")
        {
            UserId = userId;
        }
    }
}

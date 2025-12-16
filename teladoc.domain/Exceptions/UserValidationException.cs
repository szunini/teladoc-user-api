namespace teladoc.domain.Exceptions
{
    public class UserValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public UserValidationException(string field, string message)
        {
            Errors = new Dictionary<string, string[]>
            {
                [field] = new[] { message }
            };
        }

        public UserValidationException(IDictionary<string, string[]> errors)
        {
            Errors = errors;
        }
    }
}
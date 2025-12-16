using System.ComponentModel.DataAnnotations;

namespace teladoc.domain.DTOs
{
    /// <summary>
    /// DTO to replate all values of the user entity
    /// </summary>
    public class UpdateUserRequest
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = null!;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(100)]
        public string? NickName { get; set; }

        [Range(0, int.MaxValue)]
        public int? FriendCount { get; set; }
    }
}

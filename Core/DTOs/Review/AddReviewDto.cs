using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Review
{
    public class AddReviewDto
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }
        [Required, MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters.")]
        public string Comment { get; set; }
    }
}
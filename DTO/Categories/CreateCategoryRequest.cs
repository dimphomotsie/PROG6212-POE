using System.ComponentModel.DataAnnotations;

namespace RaceDay_API.DTO.Categories
{
    public class CreateCategoryRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}

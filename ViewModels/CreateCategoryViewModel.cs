using System.ComponentModel.DataAnnotations;

namespace CourseHub.ViewModels
{
    public class CreateCategoryViewModel
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}

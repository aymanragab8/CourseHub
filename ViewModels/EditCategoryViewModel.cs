using System.ComponentModel.DataAnnotations;

namespace CourseHub.ViewModels
{
    public class EditCategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}

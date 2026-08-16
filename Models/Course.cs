using System.ComponentModel.DataAnnotations;

namespace CourseHub.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(0, 100000)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime EnrollmentDeadline { get; set; }


        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public string InstructorId { get; set; } = string.Empty;

        public ApplicationUser Instructor { get; set; } = null!;

        public ICollection<Enrollment> Enrollments { get; set; }
            = new List<Enrollment>();
    }
}
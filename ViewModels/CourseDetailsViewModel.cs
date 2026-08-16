namespace CourseHub.ViewModels
{
    public class CourseDetailsViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string InstructorName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime EnrollmentDeadline { get; set; }

        public bool IsEnrolled { get; set; }

        public bool CanManage { get; set; }
    }
}
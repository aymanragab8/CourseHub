namespace CourseHub.ViewModels
{
    public class MyEnrollmentViewModel
    {
        public int EnrollmentId { get; set; }

        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string InstructorName { get; set; } = string.Empty;

        public DateTime EnrolledAt { get; set; }
    }
}
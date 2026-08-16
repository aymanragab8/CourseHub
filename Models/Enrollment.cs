namespace CourseHub.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public string StudentId { get; set; } = string.Empty;
        public ApplicationUser Student { get; set; } = null!;
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
    }
}

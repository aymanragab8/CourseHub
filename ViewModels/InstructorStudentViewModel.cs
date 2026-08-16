namespace CourseHub.ViewModels
{
    public class InstructorStudentViewModel
    {
        public string StudentId { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public List<string> Courses { get; set; } = new();
    }
}
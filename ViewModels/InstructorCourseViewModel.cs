namespace CourseHub.ViewModels
{
    public class InstructorCourseViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int StudentsCount { get; set; }
    }
}
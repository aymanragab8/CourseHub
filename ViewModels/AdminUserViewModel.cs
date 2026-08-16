namespace CourseHub.ViewModels
{
    public class AdminUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? UserName { get; set; }

        public string CurrentRole { get; set; } = string.Empty;
    }
}
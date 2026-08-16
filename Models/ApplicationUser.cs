using Microsoft.AspNetCore.Identity;

namespace CourseHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}

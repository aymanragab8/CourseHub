using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CourseHub.ViewModels
{
    public class ChangeUserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string SelectedRole { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }
}
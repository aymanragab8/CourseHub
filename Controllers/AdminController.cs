using CourseHub.Data;
using CourseHub.Models;
using CourseHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =====================================================
        // Dashboard
        // =====================================================

        public async Task<IActionResult> Dashboard()
        {
            var students = await _userManager.GetUsersInRoleAsync("Student");

            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");

            var totalUsers = await _userManager.Users.CountAsync();

            var totalCourses = await _context.Courses.CountAsync();

            var totalEnrollments = await _context.Enrollments.CountAsync();

            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                TotalStudents = students.Count,
                TotalInstructors = instructors.Count,
                TotalCourses = totalCourses,
                TotalEnrollments = totalEnrollments
            };

            return View(model);
        }


        // =====================================================
        // Students
        // =====================================================

        public async Task<IActionResult> Students()
        {
            var users = await _userManager.GetUsersInRoleAsync("Student");

            var model = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    UserName = user.UserName,
                    CurrentRole = "Student"
                });
            }

            return View(model);
        }


        // =====================================================
        // Instructors
        // =====================================================

        public async Task<IActionResult> Instructors()
        {
            var users = await _userManager.GetUsersInRoleAsync("Instructor");

            var model = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    UserName = user.UserName,
                    CurrentRole = "Instructor"
                });
            }

            return View(model);
        }


        // =====================================================
        // Courses
        // =====================================================

        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }


        // =====================================================
        // Change User Role
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(string userId, string newRole)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["Error"] = "Invalid user.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (string.IsNullOrWhiteSpace(newRole))
            {
                TempData["Error"] = "Invalid role.";
                return RedirectToAction(nameof(Dashboard));
            }

            var allowedRoles = new[]
            {
                "Student",
                "Instructor"
            };

            if (!allowedRoles.Contains(newRole))
            {
                TempData["Error"] = "Invalid role selected.";
                return RedirectToAction(nameof(Dashboard));
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "You cannot change your own role.";
                return RedirectToAction(nameof(Dashboard));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.Where(r => r == "Student" || r == "Instructor").ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = "Failed to remove the current role.";

                    return RedirectToAction(nameof(Dashboard));
                }
            }

            var addResult =
                await _userManager.AddToRoleAsync(user, newRole);

            if (!addResult.Succeeded)
            {
                TempData["Error"] = "Failed to assign the new role.";

                return RedirectToAction(nameof(Dashboard));
            }

            TempData["Success"] = $"User role changed to {newRole} successfully.";

            return RedirectToAction(nameof(Dashboard));
        }


        // =====================================================
        // Enrollments
        // =====================================================

        public async Task<IActionResult> Enrollments()
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .AsNoTracking()
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            return View(enrollments);
        }


        // =====================================================
        // Remove Enrollment
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.Id == id);

            if (enrollment == null)
            {
                TempData["Error"] = "Enrollment not found.";

                return RedirectToAction(nameof(Enrollments));
            }

            _context.Enrollments.Remove(enrollment);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Enrollment removed successfully.";

            return RedirectToAction(nameof(Enrollments));
        }
    }
}
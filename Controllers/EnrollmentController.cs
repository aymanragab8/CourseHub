using CourseHub.Data;
using CourseHub.Models;
using CourseHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Controllers
{
    [Authorize(Roles = "Student")]
    public class EnrollmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EnrollmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================
        // My Courses
        // =========================

        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var courses = await _context.Enrollments
                .Where(e => e.StudentId == user.Id)
                .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .AsNoTracking()
                .OrderByDescending(e => e.EnrolledAt)
                .Select(e => new MyEnrollmentViewModel
                {
                    EnrollmentId = e.Id,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    ImageUrl = e.Course.ImageUrl,
                    InstructorName = e.Course.Instructor.FullName,
                    EnrolledAt = e.EnrolledAt
                })
                .ToListAsync();

            return View(courses);
        }

        // =========================
        // Enroll
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                return NotFound();

            // =========================
            // Check Enrollment Deadline
            // =========================

            if (course.EnrollmentDeadline <= DateTime.Now)
            {
                TempData["Error"] = "Enrollment is closed for this course.";

                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            // =========================
            // Check Existing Enrollment
            // =========================

            var alreadyEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Error"] = "You are already enrolled in this course.";

                return RedirectToAction("Details", "Courses", new { id = courseId });
            }

            // =========================
            // Create Enrollment
            // =========================

            var enrollment = new Enrollment
            {
                StudentId = user.Id,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);

            await _context.SaveChangesAsync();

            TempData["Success"] = "You have successfully enrolled in the course.";

            return RedirectToAction(nameof(MyCourses));
        }

        // =========================
        // Unenroll
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unenroll(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId == user.Id && e.CourseId == courseId);

            if (enrollment == null)
                return NotFound();

            _context.Enrollments.Remove(enrollment);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyCourses));
        }
    }
}
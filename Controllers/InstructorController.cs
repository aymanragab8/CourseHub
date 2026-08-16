using CourseHub.Data;
using CourseHub.Models;
using CourseHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Controllers
{
    [Authorize(Roles = "Instructor")]
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InstructorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // =========================
        // Dashboard
        // =========================

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();


            var myCoursesCount = await _context.Courses.CountAsync(c => c.InstructorId == user.Id);


            var totalEnrollments = await _context.Enrollments.CountAsync(e => e.Course.InstructorId == user.Id);


            var studentsCount = await _context.Enrollments
                .Where(e => e.Course.InstructorId == user.Id)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();


            var model = new InstructorDashboardViewModel
            {
                MyCoursesCount = myCoursesCount,
                StudentsCount = studentsCount,
                TotalEnrollments = totalEnrollments
            };


            return View(model);
        }


        // =========================
        // My Courses
        // =========================

        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var courses = await _context.Courses
                .Include(c => c.Category)
                .Where(c => c.InstructorId == user.Id)
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }


        public async Task<IActionResult> Students()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var students = await _context.Enrollments
                .Where(e => e.Course.InstructorId == user.Id)
                .Include(e => e.Student)
                .Include(e => e.Course)
                .AsNoTracking()
                .GroupBy(e => new
                {
                    e.StudentId,
                    e.Student.FullName,
                    e.Student.Email
                })
                .Select(g => new InstructorStudentViewModel
                {
                    StudentId = g.Key.StudentId,
                    StudentName = g.Key.FullName,
                    Email = g.Key.Email,

                    Courses = g.Select(e => e.Course.Title).Distinct().ToList()
                })
                .OrderBy(s => s.StudentName)
                .ToListAsync();

            return View(students);
        }


        // =========================
        // Enrollments
        // =========================

        public async Task<IActionResult> Enrollments()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var enrollments = await _context.Enrollments
                .Where(e => e.Course.InstructorId == user.Id)
                .Include(e => e.Student)
                .Include(e => e.Course)
                .AsNoTracking()
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            return View(enrollments);
        }
    }
}
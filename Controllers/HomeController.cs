using CourseHub.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var coursesCount = await _context.Courses.CountAsync();
            var categoriesCount = await _context.Categories.CountAsync();

            ViewBag.CoursesCount = coursesCount;
            ViewBag.CategoriesCount = categoriesCount;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
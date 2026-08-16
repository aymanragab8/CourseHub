using AutoMapper;
using CourseHub.Data;
using CourseHub.Models;
using CourseHub.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(ApplicationDbContext context, IMapper mapper, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, ILogger<CoursesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _environment = environment;
            _logger = logger;
        }


        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            const int pageSize = 6;

            if (page < 1)
                page = 1;

            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .AsNoTracking()
                .AsQueryable();


            // =========================
            // Search
            // =========================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(c =>
                    c.Title.Contains(search) ||
                    c.Description.Contains(search) ||
                    c.Instructor.FullName.Contains(search));
            }


            // =========================
            // Category Filter
            // =========================

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }


            // =========================
            // Total Courses
            // =========================

            var totalCourses = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);


            if (totalPages > 0 && page > totalPages)
                page = totalPages;


            // =========================
            // Pagination
            // =========================

            var courses = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            // =========================
            // Categories
            // =========================

            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();


            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = categories;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCourses = totalCourses;


            return View(courses);
        }


        // =========================================================
        // DETAILS
        // =========================================================

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();


            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Instructor)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);


            if (course == null)
                return NotFound();


            var model = _mapper.Map<CourseDetailsViewModel>(course);


            // =========================
            // Enrollment Deadline
            // =========================

            model.EnrollmentDeadline = course.EnrollmentDeadline;


            // =========================
            // Logged In User
            // =========================

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);


                if (user != null)
                {
                    model.IsEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == course.Id);


                    model.CanManage = User.IsInRole("Admin") ||
                        (
                            User.IsInRole("Instructor") && course.InstructorId == user.Id
                        );
                }
            }


            return View(model);
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateCourseViewModel
            {
                EnrollmentDeadline = DateTime.Now.AddDays(7)
            };


            await LoadCategories(model);


            // Admin
            if (User.IsInRole("Admin"))
            {
                await LoadInstructors(model);
            }


            return View(model);
        }


        // =========================================================
        // CREATE - POST
        // =========================================================
        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<IActionResult> Create(CreateCourseViewModel model)
        {
            _logger.LogInformation("========== CREATE POST STARTED ==========");

            _logger.LogInformation("Title: {Title}", model.Title);
            _logger.LogInformation("Image: {Image}", model.Image?.FileName);
            _logger.LogInformation("Image Size: {Size}", model.Image?.Length);

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is INVALID");

                    await LoadCategories(model);

                    if (User.IsInRole("Admin"))
                        await LoadInstructors(model);

                    return View(model);
                }

                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return Challenge();

                var course = _mapper.Map<Course>(model);

                course.CreatedAt = DateTime.UtcNow;

                if (User.IsInRole("Instructor"))
                {
                    course.InstructorId = user.Id;
                }
                else if (User.IsInRole("Admin"))
                {
                    if (string.IsNullOrEmpty(model.InstructorId))
                    {
                        ModelState.AddModelError(nameof(model.InstructorId), "Please select an instructor.");

                        await LoadCategories(model);
                        await LoadInstructors(model);

                        return View(model);
                    }

                    course.InstructorId = model.InstructorId;
                }

                // =========================
                // IMAGE
                // =========================

                if (model.Image != null && model.Image.Length > 0)
                {
                    _logger.LogInformation("IMAGE RECEIVED: {Name} - {Size}", model.Image.FileName, model.Image.Length);

                    course.ImageUrl = await SaveImage(model.Image);

                    _logger.LogInformation("IMAGE SAVED: {Url}", course.ImageUrl);
                }
                else
                {
                    course.ImageUrl = "/images/courses/default-course.jpg";

                    _logger.LogInformation("NO IMAGE - USING DEFAULT");
                }

                // =========================
                // DATABASE
                // =========================

                _context.Courses.Add(course);

                await _context.SaveChangesAsync();

                _logger.LogInformation("COURSE CREATED: {Id}", course.Id);

                TempData["Success"] = "Course created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "========== CREATE COURSE ERROR ==========");

                ModelState.AddModelError("", "Unable to create course. Check the logs for details.");

                await LoadCategories(model);

                if (User.IsInRole("Admin"))
                    await LoadInstructors(model);

                return View(model);
            }
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]

        [RequestSizeLimit(10 * 1024 * 1024)]

        [RequestFormLimits(
            MultipartBodyLengthLimit = 10 * 1024 * 1024)]

        public async Task<IActionResult> Edit(
            EditCourseViewModel model)
        {
            try
            {
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == model.Id);


                if (course == null)
                    return NotFound();


                if (!await CanManageCourse(course))
                    return Forbid();


                // =========================
                // Validation
                // =========================

                if (!ModelState.IsValid)
                {
                    model.ExistingImageUrl = course.ImageUrl;

                    await LoadCategories(model);

                    return View(model);
                }


                // =========================
                // Deadline
                // =========================

                if (model.EnrollmentDeadline <= DateTime.Now)
                {
                    ModelState.AddModelError(nameof(model.EnrollmentDeadline), "Enrollment deadline must be in the future.");


                    model.ExistingImageUrl = course.ImageUrl;


                    await LoadCategories(model);

                    return View(model);
                }


                // =========================
                // Old Image
                // =========================

                var oldImage = course.ImageUrl;


                // =========================
                // Map
                // =========================

                _mapper.Map(model, course);


                // =================================================
                // New Image
                // =================================================

                if (model.Image != null && model.Image.Length > 0)
                {
                    _logger.LogInformation("Replacing course image. FileName: {FileName}, Size: {Size}", model.Image.FileName, model.Image.Length);


                    var newImage = await SaveImage(model.Image);


                    course.ImageUrl = newImage;


                    // Delete old image
                    if (!string.IsNullOrEmpty(oldImage) && !oldImage.Contains("default-course.jpg", StringComparison.OrdinalIgnoreCase))
                    {
                        DeleteImage(oldImage);
                    }
                }
                else
                {
                    // Keep existing image
                    course.ImageUrl = oldImage;
                }


                await _context.SaveChangesAsync();


                TempData["Success"] = "Course updated successfully.";


                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while editing course. CourseId: {CourseId}", model.Id);


                ModelState.AddModelError(string.Empty, $"Unable to update course: {ex.Message}");


                var course = await _context.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == model.Id);


                if (course != null)
                {
                    model.ExistingImageUrl = course.ImageUrl;
                }


                await LoadCategories(model);

                return View(model);
            }
        }


        // =========================================================
        // DELETE - GET
        // =========================================================

        [Authorize(Roles = "Instructor,Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();


            var course =
                await _context.Courses
                    .Include(c => c.Category)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id);


            if (course == null)
                return NotFound();


            if (!await CanManageCourse(course))
                return Forbid();


            return View(course);
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [Authorize(Roles = "Instructor,Admin")]
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);


            if (course == null)
                return NotFound();


            if (!await CanManageCourse(course))
                return Forbid();


            DeleteImage(course.ImageUrl);


            _context.Courses.Remove(course);

            await _context.SaveChangesAsync();


            TempData["Success"] = "Course deleted successfully.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // LOAD CATEGORIES
        // =========================================================

        private async Task LoadCategories(CreateCourseViewModel model)
        {
            model.Categories = await _context.Categories
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c =>
                        new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        })
                    .ToListAsync();
        }


        private async Task LoadCategories(EditCourseViewModel model)
        {
            model.Categories = await _context.Categories
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .Select(c =>
                        new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        })
                    .ToListAsync();
        }


        // =========================================================
        // LOAD INSTRUCTORS
        // =========================================================

        private async Task LoadInstructors(CreateCourseViewModel model)
        {
            var instructors = await _userManager.GetUsersInRoleAsync("Instructor");


            model.Instructors = instructors
                    .OrderBy(i => i.FullName)
                    .Select(i =>
                        new SelectListItem
                        {
                            Value = i.Id,
                            Text =
                                $"{i.FullName} ({i.Email})"
                        })
                    .ToList();
        }


        // =========================================================
        // CAN MANAGE COURSE
        // =========================================================

        private async Task<bool> CanManageCourse(Course course)
        {
            // Admin can manage every course
            if (User.IsInRole("Admin"))
                return true;


            var user = await _userManager.GetUserAsync(User);


            return user != null && course.InstructorId == user.Id;
        }


        // =========================================================
        // SAVE IMAGE
        // =========================================================

        private async Task<string> SaveImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
            {
                throw new InvalidOperationException("No image was selected.");
            }

            const long maxFileSize = 10 * 1024 * 1024;

            if (image.Length > maxFileSize)
            {
                throw new InvalidOperationException("Image size cannot exceed 10 MB.");
            }

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrEmpty(webRootPath))
            {
                throw new InvalidOperationException("wwwroot path was not found.");
            }

            var folderPath = Path.Combine(webRootPath, "images", "courses");

            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid():N}{extension}";

            var filePath = Path.Combine(folderPath, fileName);

            _logger.LogInformation("ABOUT TO SAVE IMAGE: {Path}", filePath);

            await using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await image.CopyToAsync(stream);
            }

            _logger.LogInformation("IMAGE SAVED SUCCESSFULLY: {Path}", filePath);

            return $"/images/courses/{fileName}";
        }


        // =========================================================
        // DELETE IMAGE
        // =========================================================

        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;


            if (imageUrl.Contains("default-course.jpg", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }


            var fileName = Path.GetFileName(imageUrl);


            if (string.IsNullOrWhiteSpace(fileName))
                return;


            var path = Path.Combine(_environment.WebRootPath, "images", "courses", fileName);


            if (System.IO.File.Exists(path))
            {
                try
                {
                    System.IO.File.Delete(path);


                    _logger.LogInformation("Course image deleted: {Path}", path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not delete course image: {Path}", path);
                }
            }
        }
    }
}
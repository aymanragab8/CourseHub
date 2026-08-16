using CourseHub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Category → Courses
            builder.Entity<Course>()
                .HasOne(course => course.Category)
                .WithMany(category => category.Courses)
                .HasForeignKey(course => course.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Instructor → Courses
            builder.Entity<Course>()
                .HasOne(course => course.Instructor)
                .WithMany()
                .HasForeignKey(course => course.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Student → Enrollments
            builder.Entity<Enrollment>()
                .HasOne(enrollment => enrollment.Student)
                .WithMany()
                .HasForeignKey(enrollment => enrollment.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Course → Enrollments
            builder.Entity<Enrollment>()
                .HasOne(enrollment => enrollment.Course)
                .WithMany(course => course.Enrollments)
                .HasForeignKey(enrollment => enrollment.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // منع الطالب من التسجيل في نفس الكورس مرتين
            builder.Entity<Enrollment>()
                .HasIndex(enrollment =>
                    new
                    {
                        enrollment.StudentId,
                        enrollment.CourseId
                    })
                .IsUnique();
        }
    }
}

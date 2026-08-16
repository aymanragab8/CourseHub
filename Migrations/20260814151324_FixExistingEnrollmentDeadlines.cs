using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseHub.Migrations
{
    public partial class FixExistingEnrollmentDeadlines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Courses
                SET EnrollmentDeadline = '2027-01-01 00:00:00'
                WHERE EnrollmentDeadline <= GETDATE()
                   OR EnrollmentDeadline IS NULL
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
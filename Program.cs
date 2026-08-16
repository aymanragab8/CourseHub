using CourseHub.Data;
using CourseHub.Data.Seed;
using CourseHub.Mapping;
using CourseHub.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace CourseHub
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // ==========================================
            // Serilog
            // ==========================================

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(
                    "Logs/coursehub-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30)
                .CreateLogger();

            try
            {
                Log.Information("Starting CourseHub application...");

                var builder = WebApplication.CreateBuilder(args);


                // ==========================================
                // Serilog
                // ==========================================

                builder.Host.UseSerilog();


                // ==========================================
                // MVC
                // ==========================================

                builder.Services.AddControllersWithViews();


                // ==========================================
                // Database
                // ==========================================

                builder.Services.AddDbContext<ApplicationDbContext>(
                    options =>
                    {
                        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
                    });


                // ==========================================
                // Identity
                // ==========================================

                builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
                        options =>
                        {
                            options.Password.RequireDigit = true;
                            options.Password.RequireLowercase = true;
                            options.Password.RequireUppercase = true;
                            options.Password.RequireNonAlphanumeric = false;
                            options.Password.RequiredLength = 6;

                            options.User.RequireUniqueEmail = true;
                        })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();


                // ==========================================
                // Authentication Cookie
                // ==========================================

                builder.Services.ConfigureApplicationCookie(
                    options =>
                    {
                        options.LoginPath = "/Account/Login";
                        options.AccessDeniedPath = "/Account/AccessDenied";
                    });


                // ==========================================
                // AutoMapper
                // ==========================================

                builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile)));


                // ==========================================
                // FILE UPLOAD
                // ==========================================

                builder.Services.Configure<FormOptions>(
                    options =>
                    {
                        // Maximum multipart form size = 50 MB
                        options.MultipartBodyLengthLimit = 50 * 1024 * 1024;

                        // Maximum body size
                        options.ValueLengthLimit = 50 * 1024 * 1024;

                        options.MultipartHeadersLengthLimit = 64 * 1024;
                    });


                // ==========================================
                // Kestrel Request Size
                // ==========================================

                builder.WebHost.ConfigureKestrel(
                    options =>
                    {
                        options.Limits.MaxRequestBodySize =
                            50 * 1024 * 1024;
                    });


                // ==========================================
                // Build App
                // ==========================================

                var app = builder.Build();


                // ==========================================
                // Error Handling
                // ==========================================

                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");

                    app.UseHsts();
                }
                else
                {
                    app.UseDeveloperExceptionPage();
                }


                // ==========================================
                // Status Code Pages
                // ==========================================

                app.UseStatusCodePagesWithReExecute("/Error/StatusCode", "?statusCode={0}");


                // ==========================================
                // HTTPS
                // ==========================================

                app.UseHttpsRedirection();


                // ==========================================
                // Static Files
                // ==========================================

                app.UseStaticFiles();


                // ==========================================
                // Routing
                // ==========================================

                app.UseRouting();


                // ==========================================
                // Authentication
                // ==========================================

                app.UseAuthentication();

                app.UseAuthorization();


                // ==========================================
                // Global Exception Logging
                // ==========================================

                app.Use(async (context, next) =>
                {
                    try
                    {
                        await next();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(
                            ex,
                            "Unhandled exception while processing {Method} {Path}",
                            context.Request.Method,
                            context.Request.Path);

                        throw;
                    }
                });


                // ==========================================
                // Routes
                // ==========================================

                app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");


                // ==========================================
                // Identity Seeder
                // ==========================================

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;

                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

                    await IdentitySeeder.SeedRolesAsync(roleManager);

                    await IdentitySeeder.SeedAdminAsync(userManager);
                }


                // ==========================================
                // Run
                // ==========================================

                Log.Information("CourseHub application started successfully.");

                await app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "CourseHub application terminated unexpectedly.");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
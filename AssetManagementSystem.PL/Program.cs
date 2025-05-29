using AssetManagementSystem.BLL;
using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.BLL.Repositories;
using AssetManagementSystem.BLL.Services;
using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.DAL.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;

namespace AssetManagementSystem.PL
{
    public class Program
    {
        private static bool _isInitialized = false;
        private static string _initStatus = "Starting...";

        public static void Main(string[] args)
        {
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            Console.WriteLine($"🚀 Starting Asset Management System on port {port}");

            var builder = WebApplication.CreateBuilder(args);
            
            // Configure for Cloud Run
            builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
            builder.Configuration.AddEnvironmentVariables();
            DotNetEnv.Env.Load();

            // Configure database connection
            var connectionString = ProcessConnectionString(builder.Configuration);
            builder.Services.AddDbContext<AssetManagementDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Configure Identity
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<AssetManagementDbContext>()
            .AddDefaultTokenProviders();

            // Configure Services and Repositories
            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient<IAssetService, AssetService>();
            builder.Services.AddTransient<IAssetRepository, AssetRepository>();
            builder.Services.AddTransient<IFacilityService, FacilityService>();
            builder.Services.AddTransient<IAssetTransferService, AssetTransferService>();
            builder.Services.AddTransient<INotificationService, NotificationService>();
            builder.Services.AddScoped<UserManager<User>>();
            builder.Services.AddScoped<SignInManager<User>>();

            // Configure Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add services for MVC
            builder.Services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            // Add MemoryCache for performance
            builder.Services.AddMemoryCache();

            // Health checks
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            // Health endpoints (for Cloud Run)
            app.MapHealthChecks("/health");
            app.MapGet("/status", () => new { 
                Status = "Running",
                Initialized = _isInitialized,
                InitStatus = _initStatus,
                Port = port,
                Timestamp = DateTime.UtcNow 
            });
            app.MapGet("/ready", () => Results.Ok($"Ready - Init: {_isInitialized}"));

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            // Configure routing
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Auth}/{action=Login}/{id?}");

            // Background initialization for database seeding
            _ = Task.Run(async () =>
            {
                await Task.Delay(2000);
                await InitializeDatabase(app);
            });

            Console.WriteLine($"✅ Listening on port {port}");
            app.Run();
        }

        private static async Task InitializeDatabase(WebApplication app)
        {
            try
            {
                _initStatus = "Initializing database...";
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AssetManagementDbContext>();
                
                await context.Database.EnsureCreatedAsync();
                _initStatus = "Database initialized successfully";
                _isInitialized = true;
                
                Console.WriteLine("✅ Database initialization completed");
            }
            catch (Exception ex)
            {
                _initStatus = $"Database initialization failed: {ex.Message}";
                Console.WriteLine($"⚠️ Database init failed: {ex.Message}");
            }
        }

        private static string ProcessConnectionString(IConfiguration configuration)
        {
            var connectionString = "Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};Trust Server Certificate=true;";
            
            return connectionString
                .Replace("${DB_SERVER}", Environment.GetEnvironmentVariable("DB_SERVER") ?? "assetmanagement-db.c5ukygaowo6o.eu-north-1.rds.amazonaws.com")
                .Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME") ?? "AssetManagementDB")
                .Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "admin")
                .Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "asset1234");
        }
    }
}

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
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using System.Text.Json.Serialization;

namespace AssetManagementSystem.PL
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			DotNetEnv.Env.Load();

			var builder = WebApplication.CreateBuilder(args);

			// 🔹 Add environment variables support
			builder.Configuration.AddEnvironmentVariables();

			// 🔹 Process connection string with environment variables
			var connectionString = ProcessConnectionString(builder.Configuration);

			// 🔹 Configure Email with environment variables
			ConfigureEmailService(builder);

			// Add services to the container
			builder.Services.AddControllersWithViews();
			builder.Services.AddLogging();

			// Register all your services
			RegisterServices(builder.Services);

			// 🔹 Configure Database with processed connection string
			builder.Services.AddDbContextPool<AssetManagementDbContext>(options =>
			{
				options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
				{
					sqlOptions.CommandTimeout(30);
					sqlOptions.EnableRetryOnFailure(
						maxRetryCount: 5,
						maxRetryDelay: TimeSpan.FromSeconds(30),
						errorNumbersToAdd: null);
				});
			});

			// Configure Identity
			ConfigureIdentity(builder.Services);

			// Configure other services
			ConfigureOtherServices(builder.Services);

			var app = builder.Build();

			// 🔹 Configure pipeline
			ConfigurePipeline(app);

			// 🔹 Initialize database
			await InitializeDatabase(app);

			app.Run();
		}

		private static string ProcessConnectionString(IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("DefaultConnection");

			if (string.IsNullOrEmpty(connectionString))
			{
				throw new InvalidOperationException("Connection string is not configured.");
			}

			// Replace environment variable placeholders
			connectionString = connectionString
				.Replace("${DB_SERVER}", Environment.GetEnvironmentVariable("DB_SERVER") ?? "assetmanagement-db.c5ukygaowo6o.eu-north-1.rds.amazonaws.com")
				.Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME") ?? "AssetManagementDB")
				.Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "admin")
				.Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new InvalidOperationException("DB_PASSWORD environment variable is required"));

			return connectionString;
		}

		private static void ConfigureEmailService(WebApplicationBuilder builder)
		{
			builder.Services.AddMailKit(optionBuilder =>
			{
				optionBuilder.UseMailKit(new MailKitOptions()
				{
					Server = builder.Configuration["Email:Server"],
					Port = int.Parse(builder.Configuration["Email:Port"]),
					SenderName = builder.Configuration["Email:SenderName"],
					SenderEmail = Environment.GetEnvironmentVariable("EMAIL_SENDER") ?? builder.Configuration["Email:SenderEmail"],
					Account = Environment.GetEnvironmentVariable("EMAIL_ACCOUNT") ?? builder.Configuration["Email:Account"],
					Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? builder.Configuration["Email:Password"],
					Security = true
				});
			});
		}

		private static void RegisterServices(IServiceCollection services)
		{
			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<IEmailSenderService, EmailService>();
			services.AddScoped<IAssetRepository, AssetRepository>();
			services.AddScoped<IAssetService, AssetService>();
			services.AddScoped<IDisposalRepository, DisposalRepository>();
			services.AddScoped<IDisposalService, DisposalService>();
			services.AddScoped<IAssetTransferRepository, AssetTransferRepository>();
			services.AddScoped<IAssetTransferService, AssetTransferService>();
			services.AddScoped<IChangeLogService, ChangeLogService>();
			services.AddScoped<IChangeLogRepository, ChangeLogRepository>();
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IFacilityRepository, FacilityRepository>();
			services.AddScoped<IFacilityService, FacilityService>();
			services.AddScoped<IDepartmentRepository, DepartmentRepository>();
			services.AddScoped<IFloorRepository, FloorRepository>();
			services.AddScoped<IRoomRepository, RoomRepository>();
			services.AddScoped<IBuildingRepository, BuildingRepository>();
			services.AddScoped<IDisbursementRepository, DisbursementRepository>();
			services.AddScoped<IStoreKeeperRepository, StoreKeeperRepository>();
			services.AddScoped<IDisbursementService, DisbursementService>();
			services.AddScoped<IReturnDocumentRepository, ReturnDocumentRepository>();
			services.AddScoped<IReturnDocumentService, ReturnDocumentService>();
			services.AddScoped<INotificationRepository, NotificationRepository>();
			services.AddScoped<INotificationService, NotificationService>();
		}

		private static void ConfigureIdentity(IServiceCollection services)
		{
			services.AddIdentity<User, IdentityRole>()
				.AddEntityFrameworkStores<AssetManagementDbContext>()
				.AddDefaultTokenProviders();

			services.Configure<IdentityOptions>(options =>
			{
				options.Password.RequiredLength = 6;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireDigit = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireUppercase = false;
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;
				options.User.RequireUniqueEmail = true;
			});
		}

		private static void ConfigureOtherServices(IServiceCollection services)
		{
			services.AddMemoryCache();
			services.Configure<MemoryCacheOptions>(options =>
			{
				options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
			});

			services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = "/Auth/Login";
				options.AccessDeniedPath = "/Auth/AccessDenied";
				options.ExpireTimeSpan = TimeSpan.FromHours(2);
				options.SlidingExpiration = true;
				options.Cookie.HttpOnly = true;
				options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Changed for flexibility
				options.Cookie.SameSite = SameSiteMode.Lax; // Add this line
			});

			services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(30);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});

			services.AddAuthorization(options =>
			{
				options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(Roles.Admin));
				options.AddPolicy("RequireManagerRole", policy => policy.RequireRole(Roles.Manager));
				options.AddPolicy("RequireSupervisorRole", policy => policy.RequireRole(Roles.Supervisor));
				options.AddPolicy("RequireDataEntryRole", policy => policy.RequireRole(Roles.DataEntry));
			});

			services.AddHttpContextAccessor();
			services.AddRazorPages().AddRazorRuntimeCompilation();
			services.AddControllersWithViews().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
			});

			// 🔹 Add Health Checks services
			services.AddHealthChecks();
		}

		private static void ConfigurePipeline(WebApplication app)
		{
			app.UseSession();

			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts(); // 🔹 Add HSTS for security
			}

			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

			// 🔹 Add health check endpoint (this will now work)
			app.MapHealthChecks("/health");

			app.MapControllers();
			app.MapStaticAssets();
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Auth}/{action=Login}/{id?}");
		}

		private static async Task InitializeDatabase(WebApplication app)
		{
			using var scope = app.Services.CreateScope();
			var services = scope.ServiceProvider;

			try
			{
				var context = services.GetRequiredService<AssetManagementDbContext>();
				var userManager = services.GetRequiredService<UserManager<User>>();
				var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

				await context.Database.EnsureCreatedAsync();

				await IdentitySeeder.SeedRolesAndAdmin(userManager, roleManager);
				await StoreKeeperSeeder.SeedStoreKeepersAsync(context);
			}
			catch (Exception ex)
			{
				var logger = services.GetRequiredService<ILogger<Program>>();
				logger.LogError(ex, "An error occurred while initializing the database.");

				// 🔹 Don't throw in production - log and continue
				if (app.Environment.IsDevelopment())
				{
					throw;
				}
			}
		}
	}
}
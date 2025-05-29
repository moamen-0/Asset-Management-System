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
			Console.WriteLine("🚀 Starting Asset Management System...");
			
			try
			{
				// Load environment variables from .env file if it exists
				try
				{
					DotNetEnv.Env.Load();
					Console.WriteLine("✅ Environment variables loaded from .env file");
				}
				catch
				{
					Console.WriteLine("ℹ️ No .env file found, using environment variables only");
				}

				var builder = WebApplication.CreateBuilder(args);

				// 🔹 Configure port for Cloud Run FIRST
				var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
				Console.WriteLine($"🌐 Configuring application to listen on port: {port}");
				
				// Configure URLs before building
				builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

				// 🔹 Add environment variables support
				builder.Configuration.AddEnvironmentVariables();

				// 🔹 Configure logging for better debugging
				builder.Logging.ClearProviders();
				builder.Logging.AddConsole();
				builder.Logging.SetMinimumLevel(LogLevel.Information);

				Console.WriteLine("📋 Starting service configuration...");

				// 🔹 Add basic services first
				builder.Services.AddControllers();
				builder.Services.AddHealthChecks();
				Console.WriteLine("✅ Basic services configured");

				// 🔹 Configure Database with connection string processing
				try
				{
					var connectionString = ProcessConnectionString(builder.Configuration);
					Console.WriteLine("🗄️ Database connection string processed successfully");

					builder.Services.AddDbContextPool<AssetManagementDbContext>(options =>
					{
						options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
						{
							sqlOptions.CommandTimeout(30);
							sqlOptions.EnableRetryOnFailure(
								maxRetryCount: 3,
								maxRetryDelay: TimeSpan.FromSeconds(10),
								errorNumbersToAdd: null);
						});
					});
					Console.WriteLine("✅ Database context configured");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ Database configuration failed: {ex.Message}");
					throw;
				}

				// 🔹 Configure Email services (with error handling)
				try
				{
					ConfigureEmailService(builder);
					Console.WriteLine("✅ Email service configured");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"⚠️ Email service configuration failed (continuing): {ex.Message}");
				}

				// 🔹 Register application services
				try
				{
					RegisterServices(builder.Services);
					Console.WriteLine("✅ Application services registered");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ Service registration failed: {ex.Message}");
					throw;
				}

				// 🔹 Configure Identity
				try
				{
					ConfigureIdentity(builder.Services);
					Console.WriteLine("✅ Identity services configured");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ Identity configuration failed: {ex.Message}");
					throw;
				}

				// 🔹 Configure other services
				try
				{
					ConfigureOtherServices(builder.Services);
					Console.WriteLine("✅ Additional services configured");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ Additional services configuration failed: {ex.Message}");
					throw;
				}

				Console.WriteLine("🔨 Building application...");
				var app = builder.Build();
				Console.WriteLine("✅ Application built successfully");

				// 🔹 Configure pipeline
				try
				{
					ConfigurePipeline(app);
					Console.WriteLine("✅ Request pipeline configured");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ Pipeline configuration failed: {ex.Message}");
					throw;
				}

				// 🔹 Initialize database (with timeout for Cloud Run)
				try
				{
					Console.WriteLine("🗄️ Initializing database...");
					await InitializeDatabase(app);
					Console.WriteLine("✅ Database initialized successfully");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"⚠️ Database initialization failed (continuing): {ex.Message}");
					// Don't throw - let the app start even if DB init fails
				}

				Console.WriteLine($"🎉 Application starting on port: {port}");
				Console.WriteLine($"🌐 Health endpoint: http://0.0.0.0:{port}/health");
				
				app.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"💥 Application failed to start: {ex.Message}");
				Console.WriteLine($"📋 Stack trace: {ex.StackTrace}");
				Environment.Exit(1);
			}
		}
		private static string ProcessConnectionString(IConfiguration configuration)
		{
			var connectionString = configuration.GetConnectionString("DefaultConnection");

			if (string.IsNullOrEmpty(connectionString))
			{
				throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
			}

			// Replace environment variable placeholders
			connectionString = connectionString
				.Replace("${DB_SERVER}", Environment.GetEnvironmentVariable("DB_SERVER") ?? "assetmanagement-db.c5ukygaowo6o.eu-north-1.rds.amazonaws.com")
				.Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME") ?? "AssetManagementDB")
				.Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "admin")
				.Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new InvalidOperationException("DB_PASSWORD environment variable is required"));

			Console.WriteLine($"🔗 Connection string configured for server: {Environment.GetEnvironmentVariable("DB_SERVER") ?? "default"}");
			return connectionString;
		}
		private static void ConfigureEmailService(WebApplicationBuilder builder)
		{
			builder.Services.AddMailKit(optionBuilder =>
			{
				var emailPort = builder.Configuration["Email:Port"];
				optionBuilder.UseMailKit(new MailKitOptions()
				{
					Server = builder.Configuration["Email:Server"] ?? "smtp.gmail.com",
					Port = int.Parse(emailPort ?? "587"),
					SenderName = builder.Configuration["Email:SenderName"] ?? "Asset Management System",
					SenderEmail = Environment.GetEnvironmentVariable("EMAIL_SENDER") ?? builder.Configuration["Email:SenderEmail"] ?? "",
					Account = Environment.GetEnvironmentVariable("EMAIL_ACCOUNT") ?? builder.Configuration["Email:Account"] ?? "",
					Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD") ?? builder.Configuration["Email:Password"] ?? "",
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
			app.UseHsts();
		}
		else
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseStaticFiles();
		app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();

		// Health check endpoint
		app.MapHealthChecks("/health");

		// Add a simple status endpoint for testing
		app.MapGet("/status", () => new { 
			Status = "Running", 
			Timestamp = DateTime.UtcNow, 
			Environment = app.Environment.EnvironmentName,
			Port = Environment.GetEnvironmentVariable("PORT") ?? "8080"
		});

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
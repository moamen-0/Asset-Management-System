using AssetManagementSystem.BLL;
using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.BLL.Repositories;
using AssetManagementSystem.BLL.Services;
using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.DAL.Utilities;
using AssetManagementSystem.PL.Mapping;
using DocumentFormat.OpenXml.InkML;
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
            var builder = WebApplication.CreateBuilder(args);


			builder.Services.AddMailKit(optionBuilder =>
			{
				optionBuilder.UseMailKit(new MailKitOptions()
				{
					//configure mailkit options here
					Server = builder.Configuration["Email:Server"],
					Port = int.Parse(builder.Configuration["Email:Port"]),
					SenderName = builder.Configuration["Email:SenderName"],
					SenderEmail = builder.Configuration["Email:SenderEmail"],
					// can be optional with no authentication 
					Account = builder.Configuration["Email:Account"],
					Password = builder.Configuration["Email:Password"],
					// enable ssl or tls
					Security = true
				});
			});
			// Add services to the container.
			builder.Services.AddControllersWithViews();
			builder.Services.AddLogging();
			builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
			builder.Services.AddScoped<IEmailSenderService, EmailService>();
			builder.Services.AddScoped<IAssetRepository, AssetRepository>();
			builder.Services.AddScoped<IAssetService, AssetService>();
			builder.Services.AddScoped<IDisposalRepository, DisposalRepository>();
			builder.Services.AddScoped<IDisposalService, DisposalService>();
            builder.Services.AddScoped<IAssetTransferRepository, AssetTransferRepository>();
            builder.Services.AddScoped<IAssetTransferService, AssetTransferService>();
			builder.Services.AddScoped<IChangeLogService, ChangeLogService>();
			builder.Services.AddScoped<IChangeLogRepository, ChangeLogRepository>();
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddScoped<IFacilityRepository, FacilityRepository>();
			builder.Services.AddScoped<IFacilityService, FacilityService>();
			builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
			builder.Services.AddScoped<IFloorRepository, FloorRepository>();
			builder.Services.AddScoped<IRoomRepository, RoomRepository>();
			builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
			builder.Services.AddScoped<IDisbursementRepository, DisbursementRepository>();
			builder.Services.AddScoped<IStoreKeeperRepository, StoreKeeperRepository>();
			builder.Services.AddScoped<IDisbursementService, DisbursementService>();
			builder.Services.AddScoped<IReturnDocumentRepository, ReturnDocumentRepository>();
			builder.Services.AddScoped<IReturnDocumentService, ReturnDocumentService>();
			builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
			builder.Services.AddScoped<INotificationService, NotificationService>();

			builder.Services.AddDbContextPool<AssetManagementDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			// إضافة الهوية (Identity)
			builder.Services.AddIdentity<User, IdentityRole>()
				.AddEntityFrameworkStores<AssetManagementDbContext>()
				.AddDefaultTokenProviders();
			builder.Services.AddMemoryCache();

			// Add this after AddIdentity<User, IdentityRole>()
			builder.Services.Configure<IdentityOptions>(options =>
			{
				// Password settings
				options.Password.RequiredLength = 6;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireDigit = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireUppercase = false;

				// Lockout settings
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;

				// User settings
				options.User.RequireUniqueEmail = true;
			});

			// Add caching
			builder.Services.AddMemoryCache();

			builder.Services.AddDbContextPool<AssetManagementDbContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
					sqlServerOptionsAction: sqlOptions =>
					{
						sqlOptions.CommandTimeout(30);  // Set command timeout to 30 seconds
						sqlOptions.EnableRetryOnFailure(
							maxRetryCount: 5,
							maxRetryDelay: TimeSpan.FromSeconds(30),
							errorNumbersToAdd: null);
					});
			});
			// Add this after AddMemoryCache()
			builder.Services.Configure<MemoryCacheOptions>(options =>
			{
				//options.SizeLimit = 1024;
				options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
			});

			// Update your ConfigureApplicationCookie
			builder.Services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = "/Auth/Login";
				options.AccessDeniedPath = "/Auth/AccessDenied";
				options.ExpireTimeSpan = TimeSpan.FromHours(2);
				options.SlidingExpiration = true;
				options.Cookie.HttpOnly = true;
				options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
			});

			builder.Services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(30);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});

			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("RequireAdminRole",
					policy => policy.RequireRole(Roles.Admin));

				options.AddPolicy("RequireManagerRole",
					policy => policy.RequireRole(Roles.Manager));

				options.AddPolicy("RequireSupervisorRole",
					policy => policy.RequireRole(Roles.Supervisor));

				options.AddPolicy("RequireDataEntryRole",
					policy => policy.RequireRole(Roles.DataEntry));
			});

			builder.Services.AddHttpContextAccessor();


			//builder.Services.AddAutoMapper(typeof(AssetProfile));
			builder.Services.AddRazorPages().AddRazorRuntimeCompilation(); 

            builder.Services.AddControllersWithViews()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
	});
			// Add this near the top of your service configuration
			builder.Services.AddSession(options =>
			{
				options.IdleTimeout = TimeSpan.FromMinutes(30);
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true;
			});

			// Add this before app.UseAuthorization()
			var app = builder.Build();

			app.UseSession();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }


			// تشغيل الـ Seeder لإنشاء الأدوار والمستخدم الإداري
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				var userManager = services.GetRequiredService<UserManager<User>>();
				var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
				var context = services.GetRequiredService<AssetManagementDbContext>();

				await IdentitySeeder.SeedRolesAndAdmin(userManager, roleManager);
				await StoreKeeperSeeder.SeedStoreKeepersAsync(context);
			}

			app.UseStaticFiles();
			app.UseSession();
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.MapControllers();

			app.MapStaticAssets();
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Auth}/{action=Login}/{id?}");
                

            app.Run();
        }
    }
}

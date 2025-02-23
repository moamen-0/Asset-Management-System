using AssetManagementSystem.BLL;
using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.BLL.Repositories;
using AssetManagementSystem.BLL.Services;
using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.PL.Mapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
			builder.Services.AddDbContextPool<AssetManagementDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			// إضافة الهوية (Identity)
			builder.Services.AddIdentity<User, IdentityRole>()
				.AddEntityFrameworkStores<AssetManagementDbContext>()
				.AddDefaultTokenProviders();
			builder.Services.AddMemoryCache();

			// تفعيل المصادقة
			builder.Services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = "/Auth/Login";
				options.AccessDeniedPath = "/Auth/AccessDenied";
			});



			builder.Services.AddHttpContextAccessor();


			//builder.Services.AddAutoMapper(typeof(AssetProfile));
			builder.Services.AddRazorPages().AddRazorRuntimeCompilation(); 

            builder.Services.AddControllersWithViews()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
	});

			var app = builder.Build();



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

				await IdentitySeeder.SeedRolesAndAdmin(userManager, roleManager);
			}

			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}

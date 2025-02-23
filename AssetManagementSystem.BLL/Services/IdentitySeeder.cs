using AssetManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public class IdentitySeeder
	{
		public static async Task SeedRolesAndAdmin(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
		{
			try
			{
				// التحقق مما إذا كان هناك أي مستخدم Admin موجود مسبقًا
				var existingAdmin = await userManager.FindByEmailAsync("admin@admin.com");
				if (existingAdmin != null)
				{
					return; // الخروج بدون تشغيل السيدر
				}

				// إنشاء الأدوار (Roles) إذا لم تكن موجودة
				string[] roles = { "Admin", "Supervisor", "User", "Manager", "Data Entry" };
				foreach (var role in roles)
				{
					if (!await roleManager.RoleExistsAsync(role))
					{
						await roleManager.CreateAsync(new IdentityRole(role));
					}
				}

				// إضافة Admin افتراضي إذا لم يكن موجودًا
				var newAdmin = new User { UserName = "admin", Email = "admin@admin.com", FullName = "System Admin" };
				var result = await userManager.CreateAsync(newAdmin, "Admin@123");

				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(newAdmin, "Admin");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error in IdentitySeeder: {ex.Message}");
			}
		}

	}
}

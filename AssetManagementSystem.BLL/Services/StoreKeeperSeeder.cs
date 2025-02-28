using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Services
{
	public static class StoreKeeperSeeder
	{
		public static async Task SeedStoreKeepersAsync(AssetManagementDbContext context)
		{
			if (!context.StoreKeepers.Any())
			{
				var storeKeepers = new List<StoreKeeper>
		   {
			   new StoreKeeper { Name = "ناصر العدل"},
			   new StoreKeeper { Name = "نواف الحربي"},
			   new StoreKeeper { Name = "عبدالكريم السنيدي"  },
			   new StoreKeeper { Name = "خالد السهيل" }
			 
		   };

				await context.StoreKeepers.AddRangeAsync(storeKeepers);
				await context.SaveChangesAsync();
			}
		}
	}
}

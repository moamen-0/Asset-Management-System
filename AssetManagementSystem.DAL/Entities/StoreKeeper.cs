using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class StoreKeeper
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public string? AssetTypeTag { get; set; } 
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class DisbursementItem
	{
		public int Id { get; set; }
		public string AssetTag { get; set; } // "C3-101-0111899"
		public string? AssetDescription { get; set; } // "TABLE, COFFEE SMALL"
		public string? Brand { get; set; }
		public int Quantity { get; set; } = 1;
		public string? Unit { get; set; } // "وحدة"
		public decimal? UnitPrice { get; set; }
		public decimal? TotalPrice { get; set; }
		public string? DisbursementType { get; set; } // "مستديم/مستهلك"
		public int? DisbursementRequestId { get; set; }
	}
}

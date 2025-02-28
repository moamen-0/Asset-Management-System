using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class DisbursementRequest
	{
		public int Id { get; set; }
		public string? RequestNumber { get; set; } // Like "11/26275953"
		public DateTime RequestDate { get; set; }= DateTime.Now;
		public string? Department { get; set; } // "الجهة الطالبة/العيادات الخارجية"
		public string? Requester { get; set; } // "محمد عوض الغميزي"
		public string? RequesterContact { get; set; } // "6318370"
		public string? WarehouseManager { get; set; } // "ناصر العدل"
		public string? StoreKeeper { get; set; } // "خالد السهيل" - Auto-populated based on assets
		public string? AuthorityName { get; set; } // "مدير مستشفى بريدة المركزي"
		public string? AuthorityPerson { get; set; } // "حمود بن صالح الزيد"
		public List<DisbursementItem>? Items { get; set; } = new();
		public string? Status { get; set; }
		public int PageCount { get; set; } = 1;
	}
}

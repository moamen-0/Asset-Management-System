using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class ReturnDocument
	{
		public int Id { get; set; }
		public string? DocumentNumber { get; set; }
		public DateTime ReturnDate { get; set; } = DateTime.Now;
		public string? ReturningDepartment { get; set; }
		public int? DepartmentId { get; set; }
		public Department? Department { get; set; }
		public string? ResponsiblePerson { get; set; }
		public string? StoreKeeper { get; set; }
		public string WarehouseManager { get; set; } = "ناصر العدل";
		public string? ReturnCommittee { get; set; } // Dynamic based on assets
		public string AuthorityName { get; set; } = "مدير مستشفى بريدة المركزي";
		public string AuthorityPerson { get; set; } = "حمود بن صالح المزيد";
		public string? ReturnReason { get; set; } // تالف, فائض, etc.
		public virtual ICollection<ReturnDocumentItem> Items { get; set; } = new List<ReturnDocumentItem>();

	}
}

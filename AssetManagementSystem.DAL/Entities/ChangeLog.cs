using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class ChangeLog
	{
		public int Id { get; set; } // Primary Key

		[Column(TypeName = "nvarchar(255)")]
		public string? EntityName { get; set; } // اسم الكيان اللي حصل عليه التعديل (مثلاً: Asset, Disposal, Transfer)

		public string? EntityId { get; set; } // رقم العنصر اللي حصل عليه التغيير (مثلاً: AssetId)

		[Column(TypeName = "nvarchar(255)")]
		public string? ActionType { get; set; } // نوع التعديل (إضافة, تعديل, حذف)

		[Column(TypeName = "nvarchar(MAX)")]
		public string? OldValues { get; set; } // القيم القديمة (JSON)

		[Column(TypeName = "nvarchar(MAX)")]
		public string? NewValues { get; set; } // القيم الجديدة (JSON)

		public DateTime ChangeDate { get; set; } = DateTime.UtcNow; // وقت التعديل

		public string? UserId { get; set; } // المستخدم الذي قام بالتعديل
		public User? User { get; set; }
	}
}

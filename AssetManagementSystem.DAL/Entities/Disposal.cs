using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class Disposal
	{
		[Key]
		public int Id { get; set; }

		//  ربط الاستبعاد بالأصل عبر `AssetTag`
		[Required]
		[ForeignKey("Asset")]
		[Column(TypeName = "nvarchar(50)")]
		public string AssetTag { get; set; }

		public Asset? Asset { get; set; }  // العلاقة بين الأصل والاستبعاد

		[Required]
		public string DisposalType { get; set; }

		[Required]
		public DateTime DisposalDate { get; set; }

		public decimal SaleValue { get; set; } = 0;
	}
}

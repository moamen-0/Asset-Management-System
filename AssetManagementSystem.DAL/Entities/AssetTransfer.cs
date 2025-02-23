using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class AssetTransfer
	{
		public int Id { get; set; }

		

		public string? TransferType { get; set; } // داخلي أو خارجي

		public DateTime TransferDate { get; set; }

		public string? FromDepartment { get; set; }
		

		public string? ToDepartment { get; set; }

		[Required]
		[ForeignKey("Asset")]
		[Column(TypeName = "nvarchar(50)")]
		public required string AssetTag { get; set; }

		public Asset? Asset { get; set; }
		


	}
}

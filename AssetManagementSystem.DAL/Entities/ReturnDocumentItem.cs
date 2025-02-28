using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.DAL.Entities
{
	public class ReturnDocumentItem
	{
		public int Id { get; set; }
		public int? ReturnDocumentId { get; set; }
		public ReturnDocument? ReturnDocument { get; set; }
		public string? AssetTag { get; set; }
		public string? AssetDescription { get; set; }
		public string Unit { get; set; } = "";
		public int Quantity { get; set; } = 1;
	}
}

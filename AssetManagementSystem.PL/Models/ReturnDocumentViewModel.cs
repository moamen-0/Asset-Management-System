using System.ComponentModel.DataAnnotations;

namespace AssetManagementSystem.PL.Models
{
	public class ReturnDocumentViewModel
	{
		
		[Display(Name = "Return Reason")]
		public string? ReturnReason { get; set; }

		
		[Display(Name = "Assets to Return")]
		public List<string>? AssetTags { get; set; } = new();
	}
}

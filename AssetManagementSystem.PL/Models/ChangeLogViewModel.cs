using System.ComponentModel.DataAnnotations;

namespace AssetManagementSystem.PL.Models
{
	public class ChangeLogViewModel
	{
		public int Id { get; set; }
		
		[Display(Name = "Entity Name")]
		public string? EntityName { get; set; }
		
		[Display(Name = "Entity ID")]
		public string? EntityId { get; set; }
		
		[Display(Name = "Action Type")]
		public string? ActionType { get; set; }
		
		[Display(Name = "Old Values")]
		public string? OldValues { get; set; }
		
		[Display(Name = "New Values")]
		public string? NewValues { get; set; }
		
		[Display(Name = "Change Date")]
		public DateTime ChangeDate { get; set; }
		
		[Display(Name = "User ID")]
		public string? UserId { get; set; }
		
		[Display(Name = "User Full Name")]
		public string? UserFullName { get; set; }
		
		// Helper properties for UI
		public bool HasOldValues => !string.IsNullOrEmpty(OldValues);
		public bool HasNewValues => !string.IsNullOrEmpty(NewValues);
	}
}

using System.ComponentModel.DataAnnotations;

namespace AssetManagementSystem.PL.Models
{
	public class DisbursementViewModel
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "الرجاء إختيار الجهة الطالبة")]
		[Display(Name = "الجهة الطالبة")]
		public string Department { get; set; }

		[Required(ErrorMessage = "الرجاء إختيار مواد للصرف")]
		[Display(Name = "المواد للصرف")]
		public List<string> AssetTags { get; set; } = new();
	}
}

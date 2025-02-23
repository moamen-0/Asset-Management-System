using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetManagementSystem.BLL.Interfaces.IService
{
	public interface IEmailSenderService
	{
		Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
		Task SendEmailAsync(string toEmail, string subject, string body);
	}
}

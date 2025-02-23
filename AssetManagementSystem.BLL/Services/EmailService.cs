using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AssetManagementSystem.BLL.Interfaces.IService;

namespace AssetManagementSystem.BLL.Services
{
	public class EmailService : IEmailSenderService
	{
		private readonly IConfiguration _configuration;

		public EmailService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
		{
			var subject = "Password Reset Request";
			var body = $@"
            <h2>Password Reset</h2>
            <p>Please click the link below to reset your password:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>If you didn't request this, please ignore this email.</p>
            <p>This link will expire in 24 hours.</p>";

			await SendEmailAsync(toEmail, subject, body);
		}

		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			try
			{
				var smtpServer = _configuration["Email:Server"];
				var port = int.Parse(_configuration["Email:Port"]);
				var senderEmail = _configuration["Email:SenderEmail"];
				var password = _configuration["Email:Password"];

				using var message = new MailMessage(senderEmail, toEmail)
				{
					Subject = subject,
					Body = body,
					IsBodyHtml = true
				};

				using var client = new SmtpClient(smtpServer, port)
				{
					EnableSsl = true,
					Credentials = new NetworkCredential(senderEmail, password)
				};

				await client.SendMailAsync(message);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to send email", ex);
			}
		}
	}
}

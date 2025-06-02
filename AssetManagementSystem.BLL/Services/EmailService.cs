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
		}		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			try
			{
				var smtpServer = _configuration["Email:Server"];
				var portStr = _configuration["Email:Port"];
				var senderEmail = _configuration["Email:Account"];
				var password = _configuration["Email:Password"];

				if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(portStr) || 
					string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
				{
					throw new InvalidOperationException("Email configuration is incomplete. Missing: " +
						$"Server: {smtpServer ?? "null"}, Port: {portStr ?? "null"}, " +
						$"Email: {senderEmail ?? "null"}, Password: {(string.IsNullOrEmpty(password) ? "null" : "****")}");
				}

				var port = int.Parse(portStr);

				using var message = new MailMessage()
				{
					From = new MailAddress(senderEmail, _configuration["Email:SenderName"] ?? "Asset Management System"),
					Subject = subject,
					Body = body,
					IsBodyHtml = true
				};
				message.To.Add(toEmail);

				using var client = new SmtpClient(smtpServer, port)
				{
					EnableSsl = true,
					UseDefaultCredentials = false,
					Credentials = new NetworkCredential(senderEmail, password),
					DeliveryMethod = SmtpDeliveryMethod.Network,
					Timeout = 60000, // 60 seconds timeout for Gmail
					Host = smtpServer
				};

				// Additional Gmail-specific settings
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
				
				await client.SendMailAsync(message);
			}
			catch (SmtpException smtpEx)
			{
				var errorMessage = $"SMTP Error ({smtpEx.StatusCode}): {smtpEx.Message}";
				
				if (smtpEx.Message.Contains("5.7.0") || smtpEx.Message.Contains("Authentication Required"))
				{
					errorMessage += "\n\nThis error typically means:\n" +
						"1. 2-Factor Authentication is not enabled on the Gmail account\n" +
						"2. App Password is not generated or incorrect\n" +
						"3. 'Less secure app access' needs to be enabled (deprecated)\n\n" +
						"Please ensure:\n" +
						"- 2FA is enabled on the Gmail account\n" +
						"- Generate a new App Password for this application\n" +
						"- Use the App Password (not the regular Gmail password)";
				}
				
				throw new Exception(errorMessage, smtpEx);
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to send email: {ex.Message}", ex);
			}
		}
	}
}

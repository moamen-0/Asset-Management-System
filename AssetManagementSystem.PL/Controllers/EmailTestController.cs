using Microsoft.AspNetCore.Mvc;
using AssetManagementSystem.BLL.Interfaces.IService;

namespace AssetManagementSystem.PL.Controllers
{
    public class EmailTestController : Controller
    {
        private readonly IEmailSenderService _emailService;

        public EmailTestController(IEmailSenderService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult TestEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestEmail(string toEmail, string subject = "Test Email", string body = "This is a test email from Asset Management System.")
        {
            try
            {
                if (string.IsNullOrEmpty(toEmail))
                {
                    ViewBag.Error = "Please provide an email address";
                    return View();
                }

                await _emailService.SendEmailAsync(toEmail, subject, body);
                ViewBag.Success = $"Test email sent successfully to {toEmail}";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Failed to send email: {ex.Message}";
            }

            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssetManagementSystem.DAL.Data;
using System.Reflection;

namespace AssetManagementSystem.PL.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AssetManagementDbContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(AssetManagementDbContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var healthStatus = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    Database = await CheckDatabaseHealth(),
                    Uptime = GetUptime(),
                    ServerTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC")
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                
                var unhealthyStatus = new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message,
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                };

                return StatusCode(503, unhealthyStatus);
            }
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailed()
        {
            try
            {
                var detailedHealth = new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow,
                    Application = new
                    {
                        Name = "Asset Management System",
                        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
                        Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                        StartTime = GetStartTime(),
                        Uptime = GetUptime()
                    },
                    Database = await GetDatabaseDetails(),
                    System = new
                    {
                        MachineName = Environment.MachineName,
                        ProcessorCount = Environment.ProcessorCount,
                        WorkingSet = Environment.WorkingSet,
                        OSVersion = Environment.OSVersion.ToString(),
                        CLRVersion = Environment.Version.ToString()
                    }
                };

                return Ok(detailedHealth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Detailed health check failed");
                return StatusCode(503, new { Status = "Unhealthy", Error = ex.Message });
            }
        }

        private async Task<object> CheckDatabaseHealth()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (canConnect)
                {
                    return new { Status = "Connected", ResponseTime = "< 1s" };
                }
                else
                {
                    return new { Status = "Disconnected", Error = "Cannot connect to database" };
                }
            }
            catch (Exception ex)
            {
                return new { Status = "Error", Error = ex.Message };
            }
        }

        private async Task<object> GetDatabaseDetails()
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var canConnect = await _context.Database.CanConnectAsync();
                var responseTime = DateTime.UtcNow - startTime;

                if (canConnect)
                {
                    // Get some basic counts
                    var userCount = await _context.Users.CountAsync();
                    var assetCount = await _context.Assets.CountAsync();

                    return new
                    {
                        Status = "Connected",
                        ResponseTime = $"{responseTime.TotalMilliseconds:F0}ms",
                        Statistics = new
                        {
                            UserCount = userCount,
                            AssetCount = assetCount
                        },
                        Provider = _context.Database.ProviderName
                    };
                }
                else
                {
                    return new { Status = "Disconnected", Error = "Cannot connect to database" };
                }
            }
            catch (Exception ex)
            {
                return new { Status = "Error", Error = ex.Message };
            }
        }

        private static DateTime GetStartTime()
        {
            return DateTime.UtcNow.AddMilliseconds(-Environment.TickCount64);
        }

        private static string GetUptime()
        {
            var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
        }
    }
}

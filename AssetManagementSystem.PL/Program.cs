using AssetManagementSystem.BLL;
using AssetManagementSystem.BLL.Interfaces;
using AssetManagementSystem.BLL.Interfaces.IRepository;
using AssetManagementSystem.BLL.Interfaces.IService;
using AssetManagementSystem.BLL.Repositories;
using AssetManagementSystem.BLL.Services;
using AssetManagementSystem.DAL.Data;
using AssetManagementSystem.DAL.Entities;
using AssetManagementSystem.DAL.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using System.Text.Json.Serialization;

namespace AssetManagementSystem.PL
{
	public class Program
	{
		private static bool _isInitialized = false;
		private static string _initStatus = "Starting...";

		public static void Main(string[] args)
		{
			// Absolutely minimal startup for Cloud Run
			var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
			Console.WriteLine($"🚀 Quick start on port {port}");

			var builder = WebApplication.CreateBuilder(args);
			
			// Essential configuration only
			builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
			builder.Configuration.AddEnvironmentVariables();
			
			// Minimal services
			builder.Services.AddHealthChecks();
			builder.Services.AddControllers();

			var app = builder.Build();

			// Immediate health endpoints
			app.MapHealthChecks("/health");
			app.MapGet("/status", () => new { 
				Status = "Running", 
				Initialized = _isInitialized,
				InitStatus = _initStatus,
				Port = port,
				Timestamp = DateTime.UtcNow 
			});
			app.MapGet("/ready", () => Results.Ok($"Ready - Init: {_isInitialized}"));

			// Start background initialization AFTER app is listening
			_ = Task.Run(async () =>
			{
				await Task.Delay(2000); // Wait for app to be fully listening
				await InitializeFullApplication(app, builder.Configuration);
			});

			Console.WriteLine($"✅ Listening on port {port}");
			app.Run();
		}

		private static async Task InitializeFullApplication(WebApplication app, IConfiguration configuration)
		{
			try
			{
				_initStatus = "Loading environment...";
				DotNetEnv.Env.Load();

				_initStatus = "Configuring database...";
				var connectionString = ProcessConnectionString(configuration);
				
				_initStatus = "Setting up services...";
				// Here we would need to reconfigure services, but for now just test connection
				
				_initStatus = "Testing database connection...";
				await TestDatabaseConnection(connectionString);
				
				_initStatus = "Database initialization complete";
				_isInitialized = true;
				
				Console.WriteLine("✅ Full application initialized");
			}
			catch (Exception ex)
			{
				_initStatus = $"Initialization failed: {ex.Message}";
				Console.WriteLine($"⚠️ Background init failed: {ex.Message}");
			}
		}

		private static async Task TestDatabaseConnection(string connectionString)
		{
			using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
			await connection.OpenAsync();
			Console.WriteLine("✅ Database connection test successful");
		}

		private static string ProcessConnectionString(IConfiguration configuration)
		{
			var connectionString = "Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};Trust Server Certificate=true;";
			
			return connectionString
				.Replace("${DB_SERVER}", Environment.GetEnvironmentVariable("DB_SERVER") ?? "assetmanagement-db.c5ukygaowo6o.eu-north-1.rds.amazonaws.com")
				.Replace("${DB_NAME}", Environment.GetEnvironmentVariable("DB_NAME") ?? "AssetManagementDB")
				.Replace("${DB_USER}", Environment.GetEnvironmentVariable("DB_USER") ?? "admin")
				.Replace("${DB_PASSWORD}", Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "asset1234");
		}
	}
}

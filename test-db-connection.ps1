# Test Google Cloud SQL Database Connection
Write-Host "🔍 Testing Google Cloud SQL Database Connection..." -ForegroundColor Green

# Database connection details
$Server = "34.71.143.39,1433"
$Database = "AssetManagementDB"
$Username = "sqlserver"
$Password = "asset@123!"

# Build connection string
$ConnectionString = "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"

Write-Host "📊 Connection Details:" -ForegroundColor Yellow
Write-Host "  Server: $Server" -ForegroundColor White
Write-Host "  Database: $Database" -ForegroundColor White
Write-Host "  Username: $Username" -ForegroundColor White
Write-Host "  Instance: durable-office-464510-t3:us-central1:assetmanagement-db" -ForegroundColor White

# Test connection using .NET
try {
    Write-Host "`n🔌 Testing connection..." -ForegroundColor Blue
    
    # Load SqlClient
    Add-Type -AssemblyName System.Data.SqlClient
    
    # Create connection
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    
    # Try to open connection
    $connection.Open()
    
    if ($connection.State -eq 'Open') {
        Write-Host "✅ Connection successful!" -ForegroundColor Green
        
        # Test a simple query
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT @@VERSION as Version, DB_NAME() as DatabaseName, GETDATE() as CurrentTime"
        $reader = $command.ExecuteReader()
        
        if ($reader.Read()) {
            Write-Host "`n📋 Database Information:" -ForegroundColor Cyan
            Write-Host "  SQL Server Version: $($reader['Version'])" -ForegroundColor White
            Write-Host "  Current Database: $($reader['DatabaseName'])" -ForegroundColor White
            Write-Host "  Current Time: $($reader['CurrentTime'])" -ForegroundColor White
        }
        $reader.Close()
    }
    
    $connection.Close()
    Write-Host "`n🎉 Database connection test completed successfully!" -ForegroundColor Green
    
} catch {
    Write-Host "`n❌ Connection failed!" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    Write-Host "`n🔧 Troubleshooting suggestions:" -ForegroundColor Yellow
    Write-Host "  1. Verify the public IP address is correct: 34.71.143.39" -ForegroundColor White
    Write-Host "  2. Check if SQL Server authentication is enabled" -ForegroundColor White
    Write-Host "  3. Verify the username and password are correct" -ForegroundColor White
    Write-Host "  4. Ensure the instance allows connections from your IP" -ForegroundColor White
    Write-Host "  5. Check if the database 'AssetManagementDB' exists" -ForegroundColor White
}

Write-Host "`n📝 Next steps if connection is successful:" -ForegroundColor Magenta
Write-Host "  1. Update Google Cloud Secret Manager with new connection string" -ForegroundColor White
Write-Host "  2. Test the application locally with new connection" -ForegroundColor White
Write-Host "  3. Deploy to Cloud Run with updated secrets" -ForegroundColor White

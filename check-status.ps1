# Cloud Run Deployment Status Checker
Write-Host "Checking Cloud Run deployment status..." -ForegroundColor Yellow

# GitHub Actions
Write-Host ""
Write-Host "GitHub Actions:" -ForegroundColor Cyan  
Write-Host "https://github.com/moamen-00/Asset-Management-System/actions" -ForegroundColor Blue

# Check if gcloud is available
Write-Host ""
Write-Host "Cloud Run Service Check:" -ForegroundColor Cyan
try {
    $serviceUrl = gcloud run services describe asset-management-system --region=us-central1 --format="value(status.url)" 2>$null
    if ($LASTEXITCODE -eq 0 -and $serviceUrl) {
        Write-Host "Service URL: $serviceUrl" -ForegroundColor Green
        
        # Test health endpoint
        Write-Host ""
        Write-Host "Testing health endpoint..." -ForegroundColor Cyan
        try {
            $response = Invoke-WebRequest -Uri "$serviceUrl/health" -TimeoutSec 10 -UseBasicParsing
            Write-Host "Health check: SUCCESS (Status $($response.StatusCode))" -ForegroundColor Green
        } catch {
            Write-Host "Health check: FAILED - $($_.Exception.Message)" -ForegroundColor Red
        }
        
        # Test main application
        Write-Host ""
        Write-Host "Testing main application..." -ForegroundColor Cyan
        try {
            $response = Invoke-WebRequest -Uri $serviceUrl -TimeoutSec 15 -UseBasicParsing
            Write-Host "Main app: SUCCESS (Status $($response.StatusCode))" -ForegroundColor Green
            Write-Host ""
            Write-Host "APPLICATION IS READY!" -ForegroundColor Green
            Write-Host "Access at: $serviceUrl" -ForegroundColor Green
        } catch {
            Write-Host "Main app: FAILED - $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "Service not found or not accessible" -ForegroundColor Red
    }
} catch {
    Write-Host "Error checking service. Make sure gcloud CLI is installed and authenticated." -ForegroundColor Red
}

Write-Host ""
Write-Host "Useful Links:" -ForegroundColor Cyan
Write-Host "- Cloud Run Console: https://console.cloud.google.com/run" -ForegroundColor Blue
Write-Host "- Application Logs: https://console.cloud.google.com/logs" -ForegroundColor Blue

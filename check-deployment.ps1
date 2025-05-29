# Script to check Cloud Run deployment status
Write-Host "🔄 Checking Cloud Run deployment status..." -ForegroundColor Yellow

# Check GitHub Actions status
Write-Host "`n📋 Recent GitHub Actions runs:" -ForegroundColor Cyan
Write-Host "Visit: https://github.com/moamen-00/Asset-Management-System/actions" -ForegroundColor Blue

# Check Cloud Run service status
Write-Host "`n☁️ Cloud Run service status:" -ForegroundColor Cyan
try {
    gcloud run services describe asset-management-system --region=us-central1 --format="value(status.url)" 2>$null
    if ($LASTEXITCODE -eq 0) {
        $serviceUrl = gcloud run services describe asset-management-system --region=us-central1 --format="value(status.url)" 2>$null
        Write-Host "Service URL: $serviceUrl" -ForegroundColor Green
        
        # Try to check health endpoint
        Write-Host "`n🏥 Checking health endpoint..." -ForegroundColor Cyan
        try {
            $response = Invoke-WebRequest -Uri "$serviceUrl/health" -TimeoutSec 10 -UseBasicParsing
            Write-Host "Health check: ✅ Status $($response.StatusCode)" -ForegroundColor Green
        } catch {
            Write-Host "Health check: ❌ $($_.Exception.Message)" -ForegroundColor Red
        }
        
        # Check main application
        Write-Host "`n🌐 Checking main application..." -ForegroundColor Cyan
        try {
            $response = Invoke-WebRequest -Uri $serviceUrl -TimeoutSec 15 -UseBasicParsing
            Write-Host "Main app: ✅ Status $($response.StatusCode)" -ForegroundColor Green
            Write-Host "Ready to access: $serviceUrl" -ForegroundColor Green
        } catch {
            Write-Host "Main app: ❌ $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "❌ Service not found or not accessible" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Error checking Cloud Run service: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "💡 Make sure gcloud CLI is installed and you're authenticated" -ForegroundColor Yellow
}

# Check logs if gcloud is available
Write-Host "`n📋 Recent logs (if available):" -ForegroundColor Cyan
try {
    gcloud logs read "resource.type=cloud_run_revision AND resource.labels.service_name=asset-management-system" --limit=10 --format="value(timestamp,textPayload)" 2>$null
} catch {
    Write-Host "💡 To check logs: gcloud logs read `"resource.type=cloud_run_revision AND resource.labels.service_name=asset-management-system`" --limit=20" -ForegroundColor Yellow
}

Write-Host "`n🔗 Useful links:" -ForegroundColor Cyan
Write-Host "• GitHub Actions: https://github.com/moamen-00/Asset-Management-System/actions" -ForegroundColor Blue
Write-Host "• Cloud Run Console: https://console.cloud.google.com/run/detail/us-central1/asset-management-system" -ForegroundColor Blue
Write-Host "• Cloud Logs: https://console.cloud.google.com/logs/query" -ForegroundColor Blue

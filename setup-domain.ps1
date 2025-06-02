# Quick Domain Setup Script for myosool.com
# This script helps you get the exact DNS records needed

Write-Host "🌐 Domain Mapping Setup for myosool.com" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

# Check if gcloud is available
if (Get-Command gcloud -ErrorAction SilentlyContinue) {
    Write-Host "✅ Google Cloud CLI detected" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "🔧 Creating domain mapping..." -ForegroundColor Yellow
    
    # Create domain mapping
    try {
        gcloud run domain-mappings create --service asset-management-system --domain myosool.com --region us-central1
        Write-Host "✅ Domain mapping created successfully!" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠️  Domain mapping may already exist or there was an error" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "📋 Getting DNS records for GoDaddy..." -ForegroundColor Yellow
    
    # Get DNS records
    try {
        gcloud run domain-mappings describe myosool.com --region us-central1
    }
    catch {
        Write-Host "❌ Could not retrieve DNS records. Please check domain mapping status." -ForegroundColor Red
    }
}
else {
    Write-Host "⚠️  Google Cloud CLI not found. Using manual method..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📝 Manual Steps:" -ForegroundColor Cyan
Write-Host "1. Go to: https://console.cloud.google.com/run" -ForegroundColor White
Write-Host "2. Click on 'asset-management-system' service" -ForegroundColor White
Write-Host "3. Go to 'Manage Custom Domains' tab" -ForegroundColor White
Write-Host "4. Click 'ADD MAPPING'" -ForegroundColor White
Write-Host "5. Enter domain: myosool.com" -ForegroundColor White
Write-Host "6. Copy the DNS records provided" -ForegroundColor White
Write-Host "7. Add them to GoDaddy DNS management" -ForegroundColor White

Write-Host ""
Write-Host "🎯 Expected DNS Records for GoDaddy:" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Type: A,    Host: @,   Points to: 216.239.32.21" -ForegroundColor White
Write-Host "Type: A,    Host: @,   Points to: 216.239.34.21" -ForegroundColor White
Write-Host "Type: A,    Host: @,   Points to: 216.239.36.21" -ForegroundColor White
Write-Host "Type: A,    Host: @,   Points to: 216.239.38.21" -ForegroundColor White
Write-Host "Type: CNAME, Host: www, Points to: ghs.googlehosted.com" -ForegroundColor White

Write-Host ""
Write-Host "⏰ Timeline:" -ForegroundColor Yellow
Write-Host "- DNS Propagation: 5-60 minutes" -ForegroundColor White
Write-Host "- SSL Certificate: 10-60 minutes" -ForegroundColor White
Write-Host "- Total Setup Time: 30-90 minutes" -ForegroundColor White

Write-Host ""
Write-Host "🔗 Useful Links:" -ForegroundColor Cyan
Write-Host "- Cloud Run Console: https://console.cloud.google.com/run" -ForegroundColor White
Write-Host "- GoDaddy DNS: https://dcc.godaddy.com/" -ForegroundColor White
Write-Host "- Current App: https://asset-management-system-580642313220.us-central1.run.app/" -ForegroundColor White

Write-Host ""
Write-Host "🎉 After setup, your app will be available at: https://myosool.com" -ForegroundColor Green

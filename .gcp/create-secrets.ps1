# Create Google Cloud Secrets for Asset Management System
# This script creates all required secrets in Google Cloud Secret Manager

# Check if gcloud CLI is installed
if (!(Get-Command "gcloud" -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Google Cloud CLI is not installed." -ForegroundColor Red
    Write-Host "Please install it from: https://cloud.google.com/sdk/docs/install" -ForegroundColor Yellow
    exit 1
}

# Check if user is authenticated
try {
    $null = gcloud auth list --filter=status:ACTIVE --format="value(account)" 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Not authenticated with Google Cloud." -ForegroundColor Red
        Write-Host "Please run: gcloud auth login" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "❌ Not authenticated with Google Cloud." -ForegroundColor Red
    Write-Host "Please run: gcloud auth login" -ForegroundColor Yellow
    exit 1
}

Write-Host "🚀 Creating secrets in Google Cloud Secret Manager..." -ForegroundColor Green
Write-Host ""

# Function to create secret
function Create-Secret {
    param(
        [string]$SecretName,
        [string]$SecretValue,
        [string]$Description
    )
    
    Write-Host "Creating secret: $SecretName" -ForegroundColor Cyan
    
    try {
        # Create the secret with the value
        $SecretValue | gcloud secrets create $SecretName --data-file=- --replication-policy="automatic" 2>$null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Successfully created: $SecretName" -ForegroundColor Green
        } else {
            # Secret might already exist, try to add a new version
            Write-Host "⚠️  Secret $SecretName already exists, updating..." -ForegroundColor Yellow
            $SecretValue | gcloud secrets versions add $SecretName --data-file=- 2>$null
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "✅ Successfully updated: $SecretName" -ForegroundColor Green
            } else {
                Write-Host "❌ Failed to create/update: $SecretName" -ForegroundColor Red
            }
        }
    } catch {
        Write-Host "❌ Error creating secret: $SecretName - $_" -ForegroundColor Red
    }
    
    Write-Host ""
}

# Your database and email configuration
$DB_SERVER = "assetmanagement-db.c5ukygaowo6o.eu-north-1.rds.amazonaws.com"
$DB_NAME = "AssetManagementDB"
$DB_USER = "admin"
$DB_PASSWORD = "asset1234"
$EMAIL_SENDER = "momenhassan7240@gmail.com"
$EMAIL_ACCOUNT = "momenhassan7240@gmail.com"
$EMAIL_PASSWORD = "edlp uhjb bkoz ejpc"

# Create the connection string
$CONNECTION_STRING = "Server=$DB_SERVER;Database=$DB_NAME;User Id=$DB_USER;Password=$DB_PASSWORD;Encrypt=True;TrustServerCertificate=True;"

Write-Host "📝 Creating secrets with your configuration..." -ForegroundColor Yellow
Write-Host ""

# Create all secrets
Create-Secret -SecretName "db-connection-string" -SecretValue $CONNECTION_STRING -Description "Database connection string for Asset Management System"
Create-Secret -SecretName "email-sender" -SecretValue $EMAIL_SENDER -Description "Email sender address"
Create-Secret -SecretName "email-account" -SecretValue $EMAIL_ACCOUNT -Description "Email account for SMTP"
Create-Secret -SecretName "email-password" -SecretValue $EMAIL_PASSWORD -Description "Email app password for SMTP"

# Also create individual database secrets (in case you need them separately)
Create-Secret -SecretName "db-server" -SecretValue $DB_SERVER -Description "Database server endpoint"
Create-Secret -SecretName "db-name" -SecretValue $DB_NAME -Description "Database name"
Create-Secret -SecretName "db-user" -SecretValue $DB_USER -Description "Database username"
Create-Secret -SecretName "db-password" -SecretValue $DB_PASSWORD -Description "Database password"

Write-Host "🎉 All secrets created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Summary of created secrets:" -ForegroundColor Cyan
Write-Host "- db-connection-string (for Cloud Run)" -ForegroundColor White
Write-Host "- email-sender" -ForegroundColor White
Write-Host "- email-account" -ForegroundColor White  
Write-Host "- email-password" -ForegroundColor White
Write-Host "- db-server (individual component)" -ForegroundColor White
Write-Host "- db-name (individual component)" -ForegroundColor White
Write-Host "- db-user (individual component)" -ForegroundColor White
Write-Host "- db-password (individual component)" -ForegroundColor White
Write-Host ""
Write-Host "🔗 Next steps:" -ForegroundColor Yellow
Write-Host "1. Go to Google Cloud Console → Cloud Run" -ForegroundColor White
Write-Host "2. Create your service and reference these secrets" -ForegroundColor White
Write-Host "3. Use 'db-connection-string' for your main database connection" -ForegroundColor White
Write-Host ""
Write-Host "🌐 You can view your secrets at:" -ForegroundColor Cyan
Write-Host "https://console.cloud.google.com/security/secret-manager" -ForegroundColor Blue

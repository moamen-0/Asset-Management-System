# Google Cloud Run Setup Script (PowerShell)
# This script sets up your project for Cloud Run deployment

param(
    [string]$ProjectId = "",
    [string]$ServiceName = "asset-management-system",
    [string]$Region = "us-central1"
)

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Red = "`e[31m"
$Reset = "`e[0m"

Write-Host "${Green}🚀 Setting up Google Cloud Run for Asset Management System${Reset}"
Write-Host ""

# Check if gcloud CLI is installed
try {
    $null = gcloud --version
} catch {
    Write-Host "${Red}❌ Google Cloud CLI is not installed. Please install it first:${Reset}"
    Write-Host "https://cloud.google.com/sdk/docs/install"
    exit 1
}

# Function to prompt for input
function Read-Input {
    param(
        [string]$Prompt,
        [string]$DefaultValue = ""
    )
    
    if ($DefaultValue) {
        $input = Read-Host "$Prompt [$DefaultValue]"
        if ([string]::IsNullOrWhiteSpace($input)) {
            return $DefaultValue
        }
        return $input
    } else {
        return Read-Host $Prompt
    }
}

# Function to prompt for secure input
function Read-SecureInput {
    param([string]$Prompt)
    $secureString = Read-Host $Prompt -AsSecureString
    $ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureString)
    try {
        return [System.Runtime.InteropServices.Marshal]::PtrToStringBSTR($ptr)
    } finally {
        [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($ptr)
    }
}

# Get project configuration
Write-Host "${Yellow}Please provide the following configuration:${Reset}"
Write-Host ""

if ([string]::IsNullOrWhiteSpace($ProjectId)) {
    $ProjectId = Read-Input "Google Cloud Project ID"
}
$ServiceName = Read-Input "Service Name" $ServiceName
$Region = Read-Input "Region" $Region
$DbServer = Read-Input "Database Server (Cloud SQL instance connection name)"
$DbName = Read-Input "Database Name" "AssetManagementDB"
$DbUser = Read-Input "Database User" "postgres"

Write-Host ""
Write-Host "${Green}Setting up Google Cloud Project...${Reset}"

# Set the project
gcloud config set project $ProjectId

# Enable required APIs
Write-Host "Enabling required APIs..."
gcloud services enable run.googleapis.com
gcloud services enable artifactregistry.googleapis.com
gcloud services enable secretmanager.googleapis.com
gcloud services enable sqladmin.googleapis.com

# Create Artifact Registry repository
Write-Host "Creating Artifact Registry repository..."
try {
    gcloud artifacts repositories create $ServiceName `
        --repository-format=docker `
        --location=$Region `
        --description="Asset Management System container repository"
} catch {
    Write-Host "Repository may already exist"
}

# Create secrets
Write-Host ""
Write-Host "${Green}Creating secrets in Secret Manager...${Reset}"

# Database password
$DbPassword = Read-SecureInput "Database Password"
$DbPassword | gcloud secrets create db-connection-string --data-file=-

# Email configuration
$EmailSender = Read-Input "Email Sender Address"
$EmailSender | gcloud secrets create email-sender --data-file=-

$EmailAccount = Read-Input "Email Account"
$EmailAccount | gcloud secrets create email-account --data-file=-

$EmailPassword = Read-SecureInput "Email Password"
$EmailPassword | gcloud secrets create email-password --data-file=-

# Create service account for Cloud Run
Write-Host ""
Write-Host "${Green}Creating service account...${Reset}"
try {
    gcloud iam service-accounts create "$ServiceName-sa" `
        --display-name="Asset Management System Service Account"
} catch {
    Write-Host "Service account may already exist"
}

# Grant permissions to service account
gcloud projects add-iam-policy-binding $ProjectId `
    --member="serviceAccount:$ServiceName-sa@$ProjectId.iam.gserviceaccount.com" `
    --role="roles/secretmanager.secretAccessor"

gcloud projects add-iam-policy-binding $ProjectId `
    --member="serviceAccount:$ServiceName-sa@$ProjectId.iam.gserviceaccount.com" `
    --role="roles/cloudsql.client"

Write-Host ""
Write-Host "${Green}✅ Setup completed successfully!${Reset}"
Write-Host ""
Write-Host "${Yellow}Next steps:${Reset}"
Write-Host "1. Set up your GitHub repository secrets:"
Write-Host "   - GCP_PROJECT_ID: $ProjectId"
Write-Host "   - GCP_SA_KEY: (Service account key JSON)"
Write-Host ""
Write-Host "2. Create a service account key for GitHub Actions:"
Write-Host "   gcloud iam service-accounts keys create key.json \"
Write-Host "     --iam-account=$ServiceName-sa@$ProjectId.iam.gserviceaccount.com"
Write-Host ""
Write-Host "3. Push your code to trigger the deployment pipeline"
Write-Host ""
Write-Host "${Green}Your Cloud Run service will be available at:${Reset}"
Write-Host "https://$ServiceName-<hash>-$Region.a.run.app"

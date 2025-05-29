#!/bin/bash

# Google Cloud Run Setup Script
# This script sets up your project for Cloud Run deployment

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚀 Setting up Google Cloud Run for Asset Management System${NC}"
echo ""

# Check if gcloud CLI is installed
if ! command -v gcloud &> /dev/null; then
    echo -e "${RED}❌ Google Cloud CLI is not installed. Please install it first:${NC}"
    echo "https://cloud.google.com/sdk/docs/install"
    exit 1
fi

# Function to prompt for input
prompt_input() {
    local prompt="$1"
    local var_name="$2"
    local default_value="$3"
    
    if [ -n "$default_value" ]; then
        read -p "$prompt [$default_value]: " input
        eval "$var_name=\"${input:-$default_value}\""
    else
        read -p "$prompt: " input
        eval "$var_name=\"$input\""
    fi
}

# Get project configuration
echo -e "${YELLOW}Please provide the following configuration:${NC}"
echo ""

prompt_input "Google Cloud Project ID" PROJECT_ID
prompt_input "Service Name" SERVICE_NAME "asset-management-system"
prompt_input "Region" REGION "us-central1"
prompt_input "Database Server (Cloud SQL instance connection name)" DB_SERVER
prompt_input "Database Name" DB_NAME "AssetManagementDB"
prompt_input "Database User" DB_USER "postgres"

echo ""
echo -e "${GREEN}Setting up Google Cloud Project...${NC}"

# Set the project
gcloud config set project $PROJECT_ID

# Enable required APIs
echo "Enabling required APIs..."
gcloud services enable run.googleapis.com
gcloud services enable artifactregistry.googleapis.com
gcloud services enable secretmanager.googleapis.com
gcloud services enable sqladmin.googleapis.com

# Create Artifact Registry repository
echo "Creating Artifact Registry repository..."
gcloud artifacts repositories create $SERVICE_NAME \
    --repository-format=docker \
    --location=$REGION \
    --description="Asset Management System container repository" || echo "Repository may already exist"

# Create secrets
echo ""
echo -e "${GREEN}Creating secrets in Secret Manager...${NC}"

# Database password
echo "Please enter database password:"
read -s DB_PASSWORD
echo $DB_PASSWORD | gcloud secrets create db-connection-string --data-file=-

# Email configuration
prompt_input "Email Sender Address" EMAIL_SENDER
echo $EMAIL_SENDER | gcloud secrets create email-sender --data-file=-

prompt_input "Email Account" EMAIL_ACCOUNT
echo $EMAIL_ACCOUNT | gcloud secrets create email-account --data-file=-

echo "Please enter email password:"
read -s EMAIL_PASSWORD
echo $EMAIL_PASSWORD | gcloud secrets create email-password --data-file=-

# Create service account for Cloud Run
echo ""
echo -e "${GREEN}Creating service account...${NC}"
gcloud iam service-accounts create $SERVICE_NAME-sa \
    --display-name="Asset Management System Service Account" || echo "Service account may already exist"

# Grant permissions to service account
gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$SERVICE_NAME-sa@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/secretmanager.secretAccessor"

gcloud projects add-iam-policy-binding $PROJECT_ID \
    --member="serviceAccount:$SERVICE_NAME-sa@$PROJECT_ID.iam.gserviceaccount.com" \
    --role="roles/cloudsql.client"

echo ""
echo -e "${GREEN}✅ Setup completed successfully!${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo "1. Set up your GitHub repository secrets:"
echo "   - GCP_PROJECT_ID: $PROJECT_ID"
echo "   - GCP_SA_KEY: (Service account key JSON)"
echo ""
echo "2. Create a service account key for GitHub Actions:"
echo "   gcloud iam service-accounts keys create key.json \\"
echo "     --iam-account=$SERVICE_NAME-sa@$PROJECT_ID.iam.gserviceaccount.com"
echo ""
echo "3. Push your code to trigger the deployment pipeline"
echo ""
echo -e "${GREEN}Your Cloud Run service will be available at:${NC}"
echo "https://$SERVICE_NAME-<hash>-$REGION.a.run.app"

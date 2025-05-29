#!/bin/bash

# AWS Parameter Store Setup Script for App Runner
# This script creates secure parameters for AWS App Runner

set -e

# Configuration
REGION="us-east-1"
ENVIRONMENT="production" 
APP_NAME="asset-management"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚀 Setting up AWS Parameter Store for App Runner${NC}"
echo -e "${YELLOW}Environment: ${ENVIRONMENT}${NC}"
echo -e "${YELLOW}Region: ${REGION}${NC}"
echo ""

# Function to create secure parameter
create_parameter() {
    local name=$1
    local description=$2
    local value=$3
    local type=${4:-"SecureString"}
    
    echo -e "Creating parameter: ${YELLOW}${name}${NC}"
    
    aws ssm put-parameter \
        --region "${REGION}" \
        --name "${name}" \
        --description "${description}" \
        --value "${value}" \
        --type "${type}" \
        --overwrite \
        --tags "Key=Environment,Value=${ENVIRONMENT}" "Key=Application,Value=${APP_NAME}" \
        > /dev/null
    
    echo -e "${GREEN}✅ Parameter created successfully${NC}"
}

# Function to prompt for secure input
prompt_secure() {
    local prompt_text=$1
    local var_name=$2
    
    echo -n "${prompt_text}: "
    read -s value
    echo ""
    eval "${var_name}='${value}'"
}

# Function to prompt for regular input
prompt_regular() {
    local prompt_text=$1
    local var_name=$2
    local default_value=$3
    
    if [ -n "${default_value}" ]; then
        echo -n "${prompt_text} (default: ${default_value}): "
    else
        echo -n "${prompt_text}: "
    fi
    
    read value
    
    if [ -z "${value}" ] && [ -n "${default_value}" ]; then
        value="${default_value}"
    fi
    
    eval "${var_name}='${value}'"
}

# Check if AWS CLI is installed and configured
if ! command -v aws &> /dev/null; then
    echo -e "${RED}❌ AWS CLI is not installed${NC}"
    exit 1
fi

if ! aws sts get-caller-identity &> /dev/null; then
    echo -e "${RED}❌ AWS CLI is not configured${NC}"
    exit 1
fi

echo -e "${YELLOW}Please provide the following configuration values:${NC}"
echo ""

# Database Configuration
echo -e "${GREEN}Database Configuration:${NC}"
prompt_regular "Database Server (RDS endpoint)" DB_SERVER
prompt_regular "Database Name" DB_NAME "AssetManagementDB"
prompt_regular "Database User" DB_USER "admin"
prompt_secure "Database Password" DB_PASSWORD

echo ""

# Email Configuration
echo -e "${GREEN}Email Configuration:${NC}"
prompt_regular "Email Sender Address" EMAIL_SENDER
prompt_regular "Email Account" EMAIL_ACCOUNT
prompt_secure "Email Password (App Password for Gmail)" EMAIL_PASSWORD

echo ""
echo -e "${YELLOW}Creating parameters in AWS Parameter Store...${NC}"
echo ""

# Create connection string parameter
CONNECTION_STRING="Server=${DB_SERVER};Database=${DB_NAME};User Id=${DB_USER};Password=${DB_PASSWORD};Encrypt=True;TrustServerCertificate=True;"

create_parameter \
    "/${APP_NAME}/${ENVIRONMENT}/connection-string" \
    "Database connection string for ${APP_NAME}" \
    "${CONNECTION_STRING}"

# Create email parameters
create_parameter \
    "/${APP_NAME}/${ENVIRONMENT}/email-sender" \
    "Email sender address for ${APP_NAME}" \
    "${EMAIL_SENDER}" \
    "String"

create_parameter \
    "/${APP_NAME}/${ENVIRONMENT}/email-account" \
    "Email account for ${APP_NAME}" \
    "${EMAIL_ACCOUNT}" \
    "String"

create_parameter \
    "/${APP_NAME}/${ENVIRONMENT}/email-password" \
    "Email password for ${APP_NAME}" \
    "${EMAIL_PASSWORD}"

echo ""
echo -e "${GREEN}🎉 All parameters created successfully!${NC}"
echo ""

# Display created parameters
echo -e "${YELLOW}Created Parameters:${NC}"
aws ssm describe-parameters \
    --region "${REGION}" \
    --parameter-filters "Key=tag:Application,Values=${APP_NAME}" \
    --query 'Parameters[].{Name:Name,Type:Type,Description:Description}' \
    --output table

echo ""
echo -e "${YELLOW}Parameter ARNs for App Runner:${NC}"
AWS_ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo "CONNECTION_STRING: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${APP_NAME}/${ENVIRONMENT}/connection-string"
echo "EMAIL_SENDER: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${APP_NAME}/${ENVIRONMENT}/email-sender"
echo "EMAIL_ACCOUNT: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${APP_NAME}/${ENVIRONMENT}/email-account"
echo "EMAIL_PASSWORD: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${APP_NAME}/${ENVIRONMENT}/email-password"

echo ""
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Use these parameter ARNs when creating your App Runner service"
echo "2. Ensure your App Runner service role has permissions to read these parameters"
echo "3. Push your code to GitHub and create the App Runner service"
echo ""
echo -e "${GREEN}Ready for App Runner deployment! 🚀${NC}"

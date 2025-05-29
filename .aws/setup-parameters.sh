#!/bin/bash

# AWS Parameter Store Setup Script for Asset Management System
# This script creates secure parameters in AWS Systems Manager Parameter Store

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

echo -e "${GREEN}🚀 Setting up AWS Parameter Store for Asset Management System${NC}"
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

# AWS Configuration
echo -e "${GREEN}AWS Configuration:${NC}"
prompt_regular "AWS Account ID" AWS_ACCOUNT_ID
prompt_regular "ECR Repository Name" ECR_REPOSITORY "${APP_NAME}"

echo ""
echo -e "${YELLOW}Creating parameters in AWS Parameter Store...${NC}"
echo ""

# Create database parameters
create_parameter \
    "/${ENVIRONMENT}/${APP_NAME}/db-server" \
    "Database server endpoint for ${APP_NAME}" \
    "${DB_SERVER}"

create_parameter \
    "/${ENVIRONMENT}/${APP_NAME}/db-name" \
    "Database name for ${APP_NAME}" \
    "${DB_NAME}"

create_parameter \
    "/${ENVIRONMENT}/${APP_NAME}/db-user" \
    "Database username for ${APP_NAME}" \
    "${DB_USER}"

create_parameter \
    "/${ENVIRONMENT}/${APP_NAME}/db-password" \
    "Database password for ${APP_NAME}" \
    "${DB_PASSWORD}"

# Create email parameters
create_parameter \
    "/${ENVIRONMENT}/${APP_NAME}/email-sender" \
    "Email sender address for ${APP_NAME}" \
    "${EMAIL_SENDER}"

create_parameter \
    "/${ENVIRONMENT}/${APP_NAME}/email-account" \
    "Email account for ${APP_NAME}" \
    "${EMAIL_ACCOUNT}"

create_parameter \
    "/${ENVIRONMENT}/${APP_NAME}/email-password" \
    "Email password for ${APP_NAME}" \
    "${EMAIL_PASSWORD}"

# Create AWS parameters
create_parameter \
    "/${ENVIRONMENT}/${APP_NAME}/aws-account-id" \
    "AWS Account ID for ${APP_NAME}" \
    "${AWS_ACCOUNT_ID}" \
    "String"

create_parameter \
    "/${ENVIRONMENT}/${APP_NAME}/ecr-repository" \
    "ECR Repository name for ${APP_NAME}" \
    "${ECR_REPOSITORY}" \
    "String"

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
echo -e "${YELLOW}Next Steps:${NC}"
echo "1. Update your ECS task definition with the correct parameter ARNs"
echo "2. Ensure your ECS task role has permissions to read these parameters"
echo "3. Update your GitHub secrets if using EC2 deployment"
echo ""

# Generate parameter ARNs for task definition
echo -e "${YELLOW}Parameter ARNs for ECS Task Definition:${NC}"
echo "DB_SERVER: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${ENVIRONMENT}/${APP_NAME}/db-server"
echo "DB_NAME: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${ENVIRONMENT}/${APP_NAME}/db-name"
echo "DB_USER: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${ENVIRONMENT}/${APP_NAME}/db-user"
echo "DB_PASSWORD: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${ENVIRONMENT}/${APP_NAME}/db-password"
echo "EMAIL_SENDER: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${ENVIRONMENT}/${APP_NAME}/email-sender"
echo "EMAIL_ACCOUNT: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${ENVIRONMENT}/${APP_NAME}/email-account"
echo "EMAIL_PASSWORD: arn:aws:ssm:${REGION}:${AWS_ACCOUNT_ID}:parameter/${ENVIRONMENT}/${APP_NAME}/email-password"

echo ""
echo -e "${GREEN}✅ Setup complete!${NC}"

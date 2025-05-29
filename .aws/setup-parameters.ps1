# AWS Parameter Store Setup Script for Asset Management System (PowerShell)
# This script creates secure parameters in AWS Systems Manager Parameter Store

param(
    [string]$Region = "us-east-1",
    [string]$Environment = "production",
    [string]$AppName = "asset-management"
)

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Red = "`e[31m"
$Reset = "`e[0m"

Write-Host "${Green}🚀 Setting up AWS Parameter Store for Asset Management System${Reset}"
Write-Host "${Yellow}Environment: ${Environment}${Reset}"
Write-Host "${Yellow}Region: ${Region}${Reset}"
Write-Host ""

# Function to create secure parameter
function New-SecureParameter {
    param(
        [string]$Name,
        [string]$Description,
        [string]$Value,
        [string]$Type = "SecureString"
    )
    
    Write-Host "Creating parameter: ${Yellow}${Name}${Reset}"
    
    try {
        aws ssm put-parameter `
            --region $Region `
            --name $Name `
            --description $Description `
            --value $Value `
            --type $Type `
            --overwrite `
            --tags "Key=Environment,Value=${Environment}" "Key=Application,Value=${AppName}" `
            --output none
        
        Write-Host "${Green}✅ Parameter created successfully${Reset}"
    }
    catch {
        Write-Host "${Red}❌ Failed to create parameter: $_${Reset}"
        exit 1
    }
}

# Function to prompt for secure input
function Read-SecureInput {
    param([string]$Prompt)
    
    Write-Host -NoNewline "${Prompt}: "
    $SecureString = Read-Host -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecureString)
    $PlainText = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    [System.Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    return $PlainText
}

# Function to prompt for regular input
function Read-RegularInput {
    param(
        [string]$Prompt,
        [string]$Default = ""
    )
    
    if ($Default) {
        $FullPrompt = "${Prompt} (default: ${Default})"
    } else {
        $FullPrompt = $Prompt
    }
    
    Write-Host -NoNewline "${FullPrompt}: "
    $Input = Read-Host
    
    if ([string]::IsNullOrWhiteSpace($Input) -and $Default) {
        return $Default
    }
    
    return $Input
}

# Check if AWS CLI is installed and configured
try {
    aws sts get-caller-identity --output none
    Write-Host "${Green}✅ AWS CLI is configured${Reset}"
}
catch {
    Write-Host "${Red}❌ AWS CLI is not configured or not installed${Reset}"
    Write-Host "Please run 'aws configure' first"
    exit 1
}

Write-Host "${Yellow}Please provide the following configuration values:${Reset}"
Write-Host ""

# Database Configuration
Write-Host "${Green}Database Configuration:${Reset}"
$DbServer = Read-RegularInput "Database Server (RDS endpoint)"
$DbName = Read-RegularInput "Database Name" "AssetManagementDB"
$DbUser = Read-RegularInput "Database User" "admin"
$DbPassword = Read-SecureInput "Database Password"

Write-Host ""

# Email Configuration
Write-Host "${Green}Email Configuration:${Reset}"
$EmailSender = Read-RegularInput "Email Sender Address"
$EmailAccount = Read-RegularInput "Email Account"
$EmailPassword = Read-SecureInput "Email Password (App Password for Gmail)"

Write-Host ""

# AWS Configuration
Write-Host "${Green}AWS Configuration:${Reset}"
$AwsAccountId = Read-RegularInput "AWS Account ID"
$EcrRepository = Read-RegularInput "ECR Repository Name" $AppName

Write-Host ""
Write-Host "${Yellow}Creating parameters in AWS Parameter Store...${Reset}"
Write-Host ""

# Create database parameters
New-SecureParameter `
    -Name "/${Environment}/${AppName}/db-server" `
    -Description "Database server endpoint for ${AppName}" `
    -Value $DbServer

New-SecureParameter `
    -Name "/${Environment}/${AppName}/db-name" `
    -Description "Database name for ${AppName}" `
    -Value $DbName

New-SecureParameter `
    -Name "/${Environment}/${AppName}/db-user" `
    -Description "Database username for ${AppName}" `
    -Value $DbUser

New-SecureParameter `
    -Name "/${Environment}/${AppName}/db-password" `
    -Description "Database password for ${AppName}" `
    -Value $DbPassword

# Create email parameters
New-SecureParameter `
    -Name "/${Environment}/${AppName}/email-sender" `
    -Description "Email sender address for ${AppName}" `
    -Value $EmailSender

New-SecureParameter `
    -Name "/${Environment}/${AppName}/email-account" `
    -Description "Email account for ${AppName}" `
    -Value $EmailAccount

New-SecureParameter `
    -Name "/${Environment}/${AppName}/email-password" `
    -Description "Email password for ${AppName}" `
    -Value $EmailPassword

# Create AWS parameters
New-SecureParameter `
    -Name "/${Environment}/${AppName}/aws-account-id" `
    -Description "AWS Account ID for ${AppName}" `
    -Value $AwsAccountId `
    -Type "String"

New-SecureParameter `
    -Name "/${Environment}/${AppName}/ecr-repository" `
    -Description "ECR Repository name for ${AppName}" `
    -Value $EcrRepository `
    -Type "String"

Write-Host ""
Write-Host "${Green}🎉 All parameters created successfully!${Reset}"
Write-Host ""

# Display created parameters
Write-Host "${Yellow}Created Parameters:${Reset}"
try {
    aws ssm describe-parameters `
        --region $Region `
        --parameter-filters "Key=tag:Application,Values=${AppName}" `
        --query 'Parameters[].{Name:Name,Type:Type,Description:Description}' `
        --output table
}
catch {
    Write-Host "${Red}Could not display parameters. Check manually in AWS Console.${Reset}"
}

Write-Host ""
Write-Host "${Yellow}Next Steps:${Reset}"
Write-Host "1. Update your ECS task definition with the correct parameter ARNs"
Write-Host "2. Ensure your ECS task role has permissions to read these parameters"
Write-Host "3. Update your GitHub secrets if using EC2 deployment"
Write-Host ""

# Generate parameter ARNs for task definition
Write-Host "${Yellow}Parameter ARNs for ECS Task Definition:${Reset}"
Write-Host "DB_SERVER: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/db-server"
Write-Host "DB_NAME: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/db-name"
Write-Host "DB_USER: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/db-user"
Write-Host "DB_PASSWORD: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/db-password"
Write-Host "EMAIL_SENDER: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/email-sender"
Write-Host "EMAIL_ACCOUNT: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/email-account"
Write-Host "EMAIL_PASSWORD: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/email-password"

Write-Host ""
Write-Host "${Green}✅ Setup complete!${Reset}"

# Save ARNs to file for easy reference
$ArnsFile = "parameter-arns.txt"
@"
# Parameter ARNs for ECS Task Definition
DB_SERVER=arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/db-server
DB_NAME=arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/db-name
DB_USER=arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/db-user
DB_PASSWORD=arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/db-password
EMAIL_SENDER=arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/email-sender
EMAIL_ACCOUNT=arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/email-account
EMAIL_PASSWORD=arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${Environment}/${AppName}/email-password
"@ | Out-File -FilePath $ArnsFile -Encoding UTF8

Write-Host "${Yellow}Parameter ARNs saved to: ${ArnsFile}${Reset}"

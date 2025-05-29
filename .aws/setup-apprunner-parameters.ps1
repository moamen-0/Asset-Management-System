# AWS Parameter Store Setup Script for App Runner (PowerShell)
# This script creates secure parameters for AWS App Runner

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

Write-Host "${Green}🚀 Setting up AWS Parameter Store for App Runner${Reset}"
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
            --tags "Key=Environment,Value=$Environment" "Key=Application,Value=$AppName" | Out-Null
        
        Write-Host "${Green}✅ Parameter created successfully${Reset}"
    }
    catch {
        Write-Host "${Red}❌ Failed to create parameter: $($_.Exception.Message)${Reset}"
        exit 1
    }
}

# Function to prompt for secure input
function Read-SecureInput {
    param([string]$Prompt)
    
    $SecureString = Read-Host -Prompt $Prompt -AsSecureString
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
    
    $Input = Read-Host -Prompt $FullPrompt
    
    if (-not $Input -and $Default) {
        return $Default
    }
    
    return $Input
}

# Check if AWS CLI is installed and configured
try {
    $null = aws --version
} catch {
    Write-Host "${Red}❌ AWS CLI is not installed${Reset}"
    exit 1
}

try {
    $null = aws sts get-caller-identity
} catch {
    Write-Host "${Red}❌ AWS CLI is not configured${Reset}"
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
Write-Host "${Yellow}Creating parameters in AWS Parameter Store...${Reset}"
Write-Host ""

# Create connection string parameter
$ConnectionString = "Server=${DbServer};Database=${DbName};User Id=${DbUser};Password=${DbPassword};Encrypt=True;TrustServerCertificate=True;"

New-SecureParameter `
    -Name "/${AppName}/${Environment}/connection-string" `
    -Description "Database connection string for ${AppName}" `
    -Value $ConnectionString

# Create email parameters
New-SecureParameter `
    -Name "/${AppName}/${Environment}/email-sender" `
    -Description "Email sender address for ${AppName}" `
    -Value $EmailSender `
    -Type "String"

New-SecureParameter `
    -Name "/${AppName}/${Environment}/email-account" `
    -Description "Email account for ${AppName}" `
    -Value $EmailAccount `
    -Type "String"

New-SecureParameter `
    -Name "/${AppName}/${Environment}/email-password" `
    -Description "Email password for ${AppName}" `
    -Value $EmailPassword

Write-Host ""
Write-Host "${Green}🎉 All parameters created successfully!${Reset}"
Write-Host ""

# Display created parameters
Write-Host "${Yellow}Created Parameters:${Reset}"
aws ssm describe-parameters `
    --region $Region `
    --parameter-filters "Key=tag:Application,Values=$AppName" `
    --query 'Parameters[].{Name:Name,Type:Type,Description:Description}' `
    --output table

Write-Host ""
Write-Host "${Yellow}Parameter ARNs for App Runner:${Reset}"
$AwsAccountId = aws sts get-caller-identity --query Account --output text
Write-Host "CONNECTION_STRING: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${AppName}/${Environment}/connection-string"
Write-Host "EMAIL_SENDER: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${AppName}/${Environment}/email-sender"
Write-Host "EMAIL_ACCOUNT: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${AppName}/${Environment}/email-account"
Write-Host "EMAIL_PASSWORD: arn:aws:ssm:${Region}:${AwsAccountId}:parameter/${AppName}/${Environment}/email-password"

Write-Host ""
Write-Host "${Yellow}Next Steps:${Reset}"
Write-Host "1. Use these parameter ARNs when creating your App Runner service"
Write-Host "2. Ensure your App Runner service role has permissions to read these parameters"
Write-Host "3. Push your code to GitHub and create the App Runner service"
Write-Host ""
Write-Host "${Green}Ready for App Runner deployment! 🚀${Reset}"

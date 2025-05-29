# CI/CD Pipeline Setup Guide for Asset Management System

## 🚀 Overview

This guide helps you set up a complete CI/CD pipeline for your Asset Management System on AWS. You have multiple deployment options depending on your needs and preferences.

## 📋 Prerequisites

### Required Tools:
- **Git** - Version control
- **AWS CLI** - AWS command line interface
- **Docker** (optional) - For containerized deployments
- **GitHub Account** - For CI/CD workflows

### AWS Services Required:
- **EC2** (current setup) or **ECS Fargate** (containerized)
- **RDS** - SQL Server database
- **Route 53** (optional) - DNS management
- **Certificate Manager** (optional) - SSL certificates
- **Parameter Store** - Secure configuration storage

## 🏗️ Deployment Options

### Option 1: EC2 Deployment (Your Current Setup) ✅

**Best for:** Small to medium applications, cost-effective, direct server control

**What's already configured:**
- ✅ GitHub Actions workflow (`.github/workflows/deploy.yml`)
- ✅ Systemd service configuration
- ✅ Nginx reverse proxy setup
- ✅ Health monitoring
- ✅ Automatic rollback capability

**To deploy:**
1. Configure GitHub secrets (see secrets section below)
2. Push to main branch
3. GitHub Actions automatically deploys

### Option 2: ECS Fargate (Containerized) 🐳

**Best for:** Scalable applications, microservices, auto-scaling needs

**What's provided:**
- ✅ Dockerfile for containerization
- ✅ ECS task definition
- ✅ CloudFormation infrastructure template
- ✅ CodeBuild configuration

**To set up:**
1. Deploy infrastructure: `aws cloudformation deploy --template-file .aws/infrastructure.yml --stack-name asset-management-infrastructure`
2. Configure container deployment workflow
3. Update GitHub workflow to use ECS deployment

### Option 3: AWS App Runner (Serverless Containers) ☁️

**Best for:** Serverless deployment, automatic scaling, pay-per-use

**Benefits:**
- Automatic scaling to zero
- No infrastructure management
- Built-in load balancing

## 🔐 Required GitHub Secrets

### For EC2 Deployment (Current Setup):
```
AWS_ACCESS_KEY_ID        # Your AWS access key
AWS_SECRET_ACCESS_KEY    # Your AWS secret key
AWS_EC2_HOST            # Your EC2 instance IP/domain
AWS_EC2_USER            # SSH username (usually 'ec2-user')
AWS_EC2_KEY             # Your EC2 private key (PEM format)
DB_SERVER               # Your RDS endpoint
DB_PASSWORD             # Database password
EMAIL_PASSWORD          # Gmail app password
```

### For ECS Deployment:
```
AWS_ACCESS_KEY_ID        # Your AWS access key
AWS_SECRET_ACCESS_KEY    # Your AWS secret key
AWS_ACCOUNT_ID          # Your AWS account ID
ECR_REPOSITORY          # Your ECR repository name
```

## 📚 Step-by-Step Setup

### Step 1: Configure AWS Credentials

#### Option A: Use AWS CLI
```bash
aws configure
# Enter your AWS Access Key ID
# Enter your AWS Secret Access Key
# Enter your default region (e.g., us-east-1)
# Enter default output format (json)
```

#### Option B: Use IAM Roles (Recommended for EC2)
- Create an IAM role with necessary permissions
- Attach role to your EC2 instance

### Step 2: Set Up Secure Configuration

#### For EC2 Deployment:
Your current workflow handles this automatically through GitHub secrets.

#### For ECS Deployment:
```powershell
# Run the parameter setup script
.\.aws\setup-parameters.ps1
```

### Step 3: Configure Infrastructure

#### For EC2 (Already Done):
Your EC2 instance should have:
- ✅ .NET 9 runtime installed
- ✅ Nginx configured
- ✅ Systemd service configured
- ✅ Database connectivity

#### For ECS:
```bash
# Deploy the infrastructure
aws cloudformation deploy \
  --template-file .aws/infrastructure.yml \
  --stack-name asset-management-infrastructure \
  --parameter-overrides Environment=production \
  --capabilities CAPABILITY_IAM
```

### Step 4: Configure GitHub Repository

1. **Add Secrets:**
   - Go to your GitHub repository
   - Navigate to Settings → Secrets and variables → Actions
   - Add the required secrets listed above

2. **Test the Pipeline:**
   - Make a small change to your code
   - Commit and push to the main branch
   - Watch the GitHub Actions workflow run

### Step 5: Configure Domain & SSL (Optional)

#### For EC2:
```bash
# Install Certbot for Let's Encrypt
sudo yum install certbot python3-certbot-nginx

# Get SSL certificate
sudo certbot --nginx -d yourdomain.com

# Update nginx configuration
sudo systemctl restart nginx
```

#### For ECS:
- Use AWS Certificate Manager for SSL certificates
- Configure HTTPS listener on Application Load Balancer

## 🔍 Monitoring & Maintenance

### Health Checks
- **Application Health:** `https://yourdomain.com/health`
- **Detailed Health:** `https://yourdomain.com/health/detailed`

### Logs
- **EC2:** `sudo journalctl -u assetmanagement -f`
- **ECS:** CloudWatch Logs
- **Nginx:** `/var/log/nginx/assetmanagement_*.log`

### Backup Strategy
- **Database:** RDS automatic backups
- **Application:** GitHub repository + deployment backups
- **Files:** Regular backup to S3

## 🚨 Troubleshooting

### Common Issues:

#### 1. Deployment Fails
```bash
# Check service status
sudo systemctl status assetmanagement

# Check logs
sudo journalctl -u assetmanagement -n 50

# Check nginx
sudo nginx -t
sudo systemctl status nginx
```

#### 2. Database Connection Issues
- Verify RDS security groups
- Check connection string in environment variables
- Test database connectivity from EC2

#### 3. SSL Certificate Issues
```bash
# Renew certificate
sudo certbot renew

# Test nginx configuration
sudo nginx -t
```

### Rollback Procedures:

#### EC2 Rollback:
```bash
# Your workflow automatically keeps backups
sudo systemctl stop assetmanagement
sudo mv /var/www/assetmanagement/current /var/www/assetmanagement/failed_$(date +%Y%m%d_%H%M%S)
sudo mv /var/www/assetmanagement/backup_YYYYMMDD_HHMMSS /var/www/assetmanagement/current
sudo systemctl start assetmanagement
```

#### ECS Rollback:
```bash
# Rollback to previous task definition revision
aws ecs update-service \
  --cluster asset-management-cluster \
  --service asset-management-service \
  --task-definition asset-management-task:PREVIOUS_REVISION
```

## 📈 Performance Optimization

### Application Level:
- Enable response compression
- Implement caching strategies
- Optimize database queries
- Use CDN for static assets

### Infrastructure Level:
- Configure auto-scaling (ECS)
- Use Application Load Balancer health checks
- Implement CloudFront distribution
- Set up ElastiCache for session storage

## 🔒 Security Best Practices

### Network Security:
- VPC with private subnets for database
- Security groups with minimal required access
- WAF for application protection

### Application Security:
- HTTPS everywhere (SSL/TLS)
- Secure headers configuration
- Input validation and sanitization
- Regular security updates

### Access Management:
- IAM roles with least privilege principle
- MFA for AWS accounts
- Audit logs and monitoring
- Regular access reviews

## 🎯 Next Steps

1. **Choose your deployment strategy** (EC2 is already set up)
2. **Configure GitHub secrets** for your chosen strategy
3. **Test the deployment pipeline** with a small change
4. **Set up monitoring and alerting**
5. **Configure SSL certificates** for production
6. **Implement backup strategies**
7. **Set up performance monitoring**

## 📞 Support

If you encounter issues:
1. Check the logs (application, nginx, system)
2. Verify AWS service configurations
3. Test connectivity (database, external services)
4. Review GitHub Actions workflow logs
5. Check AWS CloudWatch for infrastructure issues

Your current EC2 deployment setup is production-ready and includes:
- ✅ Automated deployments
- ✅ Health checks
- ✅ Rollback capability
- ✅ Security configurations
- ✅ Monitoring and logging

You can start deploying immediately by configuring the GitHub secrets!

# Deployment Scripts for Asset Management System

## Prerequisites
Before deploying, ensure you have:

1. **AWS CLI configured** with appropriate permissions
2. **GitHub Secrets configured** (see secrets section below)
3. **Domain name** (optional, for custom domain)
4. **SSL Certificate** (optional, for HTTPS)

## Deployment Options

### Option 1: Current EC2 Deployment (Recommended for your setup)
Your current GitHub Actions workflow deploys to EC2 and is well-configured. This is suitable for:
- Small to medium applications
- Cost-effective solution
- Direct server control

### Option 2: ECS Fargate Deployment (Scalable)
For containerized deployment with auto-scaling:
```bash
# Deploy infrastructure
aws cloudformation deploy \
  --template-file .aws/infrastructure.yml \
  --stack-name asset-management-infrastructure \
  --parameter-overrides \
    Environment=production \
    DBMasterPassword=YourSecurePassword123! \
  --capabilities CAPABILITY_IAM

# Build and push Docker image
docker build -t asset-management .
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com
docker tag asset-management:latest YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/asset-management:latest
docker push YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/asset-management:latest
```

### Option 3: AWS App Runner (Serverless)
For serverless container deployment:
- Automatic scaling to zero
- Pay per use
- Managed infrastructure

## 🚨 Troubleshooting Deployment Issues

### Common Deployment Problems and Solutions

#### 1. Missing .NET 9 Runtime Error
**Problem**: Application fails to start with ".NET runtime not found" error.

**Solution**: The updated deployment script now automatically installs .NET 9 runtime. If you still encounter this issue:

```bash
# SSH into your EC2 instance
ssh -i your-key.pem ec2-user@your-ec2-ip

# Run the troubleshooting script
chmod +x /var/www/assetmanagement/.aws/troubleshoot.sh
sudo /var/www/assetmanagement/.aws/troubleshoot.sh

# Manually install .NET 9 if needed
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0 --runtime aspnetcore
export PATH="$HOME/.dotnet:$PATH"
sudo ln -sf $HOME/.dotnet/dotnet /usr/local/bin/dotnet
```

#### 2. Application Not Responding on Port 5000
**Problem**: Service is running but not accessible.

**Solution**:
```bash
# Check service status
sudo systemctl status assetmanagement

# Check application logs
sudo journalctl -u assetmanagement -f

# Verify port is listening
sudo netstat -tlnp | grep :5000

# Restart service if needed
sudo systemctl restart assetmanagement
```

#### 3. Nginx Proxy Not Working
**Problem**: Application works on port 5000 but not through nginx on port 80.

**Solution**:
```bash
# Check nginx status
sudo systemctl status nginx

# Test nginx configuration
sudo nginx -t

# Check nginx logs
sudo tail -f /var/log/nginx/error.log

# Restart nginx
sudo systemctl restart nginx
```

#### 4. Database Connection Issues
**Problem**: Application starts but database operations fail.

**Solution**:
1. Verify database connection string in `appsettings.Production.json`
2. Check AWS RDS security groups allow connections from EC2
3. Test database connectivity:
```bash
# Test SQL Server connection (replace with your values)
telnet your-rds-endpoint.amazonaws.com 1433
```

#### 5. File Upload Issues
**Problem**: File uploads fail or files are not accessible.

**Solution**:
```bash
# Check directory permissions
ls -la /var/www/assetmanagement/current/wwwroot/files/

# Fix permissions if needed
sudo chown -R ec2-user:ec2-user /var/www/assetmanagement/
sudo chmod -R 755 /var/www/assetmanagement/current/wwwroot/files/
```

### Quick Diagnostic Commands

Run these commands on your EC2 instance to quickly diagnose issues:

```bash
# 1. Check all services
sudo systemctl status assetmanagement nginx

# 2. Check application is responding
curl http://localhost:5000
curl http://localhost:80

# 3. Check recent logs
sudo journalctl -u assetmanagement -n 50 --no-pager

# 4. Check .NET installation
dotnet --version
dotnet --list-runtimes

# 5. Check listening ports
sudo netstat -tlnp | grep -E ':80|:5000'
```

### Using the Troubleshooting Script

We've included a comprehensive troubleshooting script that checks all common issues:

```bash
# SSH into your EC2 instance
ssh -i your-key.pem ec2-user@your-ec2-ip

# Download and run the troubleshooting script
curl -O https://raw.githubusercontent.com/your-repo/Asset-Management-System/main/.aws/troubleshoot.sh
chmod +x troubleshoot.sh
./troubleshoot.sh
```

This script will automatically check:
- .NET runtime installation
- Application files and permissions
- Systemd service status
- Application logs
- Port availability
- Nginx configuration
- Network connectivity

### Manual Deployment (Emergency Fallback)

If the automated deployment fails, you can deploy manually:

```bash
# 1. Build locally
dotnet publish AssetManagementSystem.PL/AssetManagementSystem.PL.csproj -c Release -o ./publish

# 2. Create deployment package
cd publish && zip -r deployment.zip .

# 3. Upload to EC2
scp -i your-key.pem deployment.zip ec2-user@your-ec2-ip:/tmp/

# 4. SSH and extract
ssh -i your-key.pem ec2-user@your-ec2-ip
sudo systemctl stop assetmanagement
cd /var/www/assetmanagement
sudo unzip -o /tmp/deployment.zip -d current/
sudo chown -R ec2-user:ec2-user current/
sudo systemctl start assetmanagement
```

### Monitoring and Logs

#### View Real-time Logs
```bash
# Application logs
sudo journalctl -u assetmanagement -f

# Nginx access logs
sudo tail -f /var/log/nginx/access.log

# Nginx error logs
sudo tail -f /var/log/nginx/error.log
```

#### Check Application Health
```bash
# Direct application health check
curl -I http://localhost:5000

# Through nginx proxy
curl -I http://your-domain.com

# Detailed response
curl -v http://your-domain.com
```

## Required GitHub Secrets

Add these secrets to your GitHub repository (Settings → Secrets and variables → Actions):

### EC2 Deployment Secrets:
- `AWS_ACCESS_KEY_ID`: Your AWS access key
- `AWS_SECRET_ACCESS_KEY`: Your AWS secret key
- `AWS_EC2_HOST`: Your EC2 instance public IP or domain
- `AWS_EC2_USER`: SSH username (usually 'ec2-user' for Amazon Linux)
- `AWS_EC2_KEY`: Your EC2 private key (PEM format)
- `DB_SERVER`: Your RDS endpoint
- `DB_PASSWORD`: Database password
- `EMAIL_PASSWORD`: Gmail app password

### ECS Deployment Secrets:
- `AWS_ACCOUNT_ID`: Your AWS account ID
- `ECR_REPOSITORY`: Your ECR repository name

## Environment Variables

The application uses these environment variables:
- `DB_SERVER`: Database server endpoint
- `DB_NAME`: Database name (AssetManagementDB)
- `DB_USER`: Database username (admin)
- `DB_PASSWORD`: Database password
- `EMAIL_SENDER`: Sender email address
- `EMAIL_ACCOUNT`: Email account
- `EMAIL_PASSWORD`: Email password
- `ASPNETCORE_ENVIRONMENT`: Environment (Production)

## SSL/HTTPS Setup

### For EC2 Deployment:
1. **Get SSL Certificate** (Let's Encrypt recommended):
```bash
sudo certbot --nginx -d yourdomain.com
```

2. **Configure Nginx** (if not already done):
```nginx
server {
    listen 443 ssl;
    server_name yourdomain.com;
    
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### For ECS Deployment:
- Use AWS Certificate Manager (ACM) for SSL certificates
- Configure HTTPS listener on Application Load Balancer

## Monitoring and Logging

### CloudWatch Integration:
- Application logs are sent to CloudWatch
- Set up alerts for error rates
- Monitor performance metrics

### Health Checks:
- Application health endpoint: `/health`
- Detailed health check: `/health/detailed`
- Database connectivity monitoring

## Backup Strategy

### Database Backups:
- RDS automatic backups (7-day retention)
- Manual snapshots before deployments
- Cross-region backup for disaster recovery

### Application Backups:
- GitHub repository serves as code backup
- Environment configurations in AWS Parameter Store
- File uploads backed up to S3

## Rollback Procedures

### EC2 Deployment:
```bash
# Your current workflow supports automatic rollback
# Keeps 3 backup versions
# Manual rollback: ssh to server and restore backup
sudo systemctl stop assetmanagement
mv /var/www/assetmanagement/current /var/www/assetmanagement/failed_$(date +%Y%m%d_%H%M%S)
mv /var/www/assetmanagement/backup_YYYYMMDD_HHMMSS /var/www/assetmanagement/current
sudo systemctl start assetmanagement
```

### ECS Deployment:
```bash
# Rollback to previous task definition
aws ecs update-service \
  --cluster asset-management-cluster \
  --service asset-management-service \
  --task-definition asset-management-task:PREVIOUS_REVISION
```

## Performance Optimization

### Application Level:
- Enable Response Compression
- Use CDN for static assets
- Database connection pooling
- Caching strategies

### Infrastructure Level:
- Auto Scaling Groups (EC2)
- Application Load Balancer health checks
- CloudFront distribution
- ElastiCache for session storage

## Security Best Practices

### Network Security:
- VPC with private subnets for database
- Security groups with minimal required access
- WAF for application protection

### Application Security:
- HTTPS everywhere
- Secure headers configuration
- Input validation and sanitization
- Regular security updates

### Access Management:
- IAM roles with least privilege
- MFA for AWS accounts
- Audit logs and monitoring
- Regular access reviews

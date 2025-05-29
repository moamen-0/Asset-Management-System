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

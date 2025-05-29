# AWS App Runner Deployment Guide

## 🚀 Overview

AWS App Runner is a serverless application service that makes it easy to deploy containerized applications. It automatically handles infrastructure management, scaling, and load balancing.

## ✅ Benefits of App Runner for Your Asset Management System

- **🔄 Automatic Scaling**: Scales to zero when not in use, scales up based on traffic
- **💰 Cost Effective**: Pay only for what you use (CPU and memory when running)
- **🛡️ Managed Security**: Automatic patching and security updates
- **🚀 Easy Deployment**: Direct integration with GitHub
- **📊 Built-in Monitoring**: Logs and metrics included
- **🔧 No Infrastructure Management**: No servers to manage

## 📋 Prerequisites

1. **AWS Account** with appropriate permissions
2. **AWS CLI** configured
3. **GitHub Repository** with your code
4. **RDS Database** (or other database service)

## 🏗️ Step-by-Step Deployment

### Step 1: Set Up AWS Parameters

Run the parameter setup script to store your secrets securely:

**For Windows (PowerShell):**
```powershell
cd "d:\mostaqel\Asset Management System"
.\.aws\setup-apprunner-parameters.ps1
```

**For Linux/Mac (Bash):**
```bash
cd "/path/to/Asset Management System"
chmod +x .aws/setup-apprunner-parameters.sh
./.aws/setup-apprunner-parameters.sh
```

This will create secure parameters in AWS Systems Manager Parameter Store for:
- Database connection string
- Email configuration
- Other sensitive settings

### Step 2: Create App Runner Service

#### Option A: Using AWS Console (Recommended for first-time)

1. **Open AWS App Runner Console**
   - Go to https://console.aws.amazon.com/apprunner
   - Click "Create service"

2. **Configure Source**
   - Source: GitHub
   - Connect to GitHub (if not already connected)
   - Repository: Select your Asset Management System repository
   - Branch: main
   - Automatic deployments: Enabled

3. **Configure Build**
   - Configuration file: Use configuration file (apprunner.yaml)
   - Or use the following settings:
     - Runtime: Docker
     - Build command: `docker build -t asset-management .`
     - Start command: `dotnet AssetManagementSystem.PL.dll`

4. **Configure Service**
   - Service name: `asset-management-system`
   - Virtual CPU: 1 vCPU
   - Memory: 2 GB
   - Port: 80
   - Health check path: `/health`

5. **Configure Environment Variables**
   Add these environment variables (using Parameter Store ARNs from Step 1):
   ```
   ASPNETCORE_ENVIRONMENT = Production
   ASPNETCORE_URLS = http://+:80
   ```

6. **Configure Secrets**
   Add these secrets (using Parameter Store ARNs from Step 1):
   ```
   ConnectionStrings__DefaultConnection = arn:aws:ssm:us-east-1:YOUR_ACCOUNT:parameter/asset-management/production/connection-string
   Email__SenderEmail = arn:aws:ssm:us-east-1:YOUR_ACCOUNT:parameter/asset-management/production/email-sender
   Email__Account = arn:aws:ssm:us-east-1:YOUR_ACCOUNT:parameter/asset-management/production/email-account
   Email__Password = arn:aws:ssm:us-east-1:YOUR_ACCOUNT:parameter/asset-management/production/email-password
   ```

7. **Review and Create**
   - Review all settings
   - Click "Create & deploy"

#### Option B: Using AWS CLI

```bash
# Create App Runner service using CLI
aws apprunner create-service \
    --service-name asset-management-system \
    --source-configuration '{
        "ImageRepository": {
            "ImageIdentifier": "public.ecr.aws/docker/library/nginx:latest",
            "ImageConfiguration": {
                "Port": "80",
                "RuntimeEnvironmentVariables": {
                    "ASPNETCORE_ENVIRONMENT": "Production",
                    "ASPNETCORE_URLS": "http://+:80"
                },
                "RuntimeEnvironmentSecrets": {
                    "ConnectionStrings__DefaultConnection": "arn:aws:ssm:us-east-1:YOUR_ACCOUNT:parameter/asset-management/production/connection-string"
                }
            },
            "ImageRepositoryType": "ECR_PUBLIC"
        },
        "AutoDeploymentsEnabled": true
    }' \
    --instance-configuration '{
        "Cpu": "1024",
        "Memory": "2048"
    }' \
    --health-check-configuration '{
        "Protocol": "HTTP",
        "Path": "/health",
        "Interval": 30,
        "Timeout": 5,
        "HealthyThreshold": 2,
        "UnhealthyThreshold": 3
    }'
```

### Step 3: Monitor Deployment

1. **Check Deployment Status**
   - In App Runner console, monitor the deployment progress
   - Initial deployment typically takes 10-15 minutes

2. **Monitor Logs**
   - App Runner provides real-time logs
   - Check for any startup errors

3. **Test Health Endpoint**
   - Once deployed, test: `https://your-app-url.awsapprunner.com/health`

### Step 4: Configure Custom Domain (Optional)

1. **Add Custom Domain**
   - In App Runner console, go to "Custom domains"
   - Add your domain name
   - Update DNS records as instructed

2. **SSL Certificate**
   - App Runner automatically provides SSL certificates
   - No additional configuration needed

## 🔧 Configuration Details

### App Runner Configuration (apprunner.yaml)

Your project includes an `apprunner.yaml` file with optimized settings:

```yaml
version: 1.0
runtime: docker
build:
  commands:
    build:
      - docker build -t asset-management .
run:
  runtime-version: latest
  env:
    - name: ASPNETCORE_ENVIRONMENT
      value: "Production"
    - name: ASPNETCORE_URLS
      value: "http://+:80"
  network:
    port: 80
    env: PORT
```

### Dockerfile Optimizations

The Dockerfile has been optimized for App Runner with:
- Multi-stage build for smaller image size
- Health check endpoint
- Non-root user for security
- Proper environment variables

## 📊 Monitoring and Maintenance

### Health Monitoring
- App Runner automatically monitors `/health` endpoint
- Automatic restarts if health checks fail
- CloudWatch metrics included

### Scaling
- Automatic scaling based on CPU and memory usage
- Scales to zero when no traffic (saves costs)
- Maximum concurrency configurable

### Logging
- Application logs available in CloudWatch
- Structured logging recommended
- Log retention configurable

## 💰 Cost Estimation

App Runner pricing is based on:
- **Active time**: $0.064 per vCPU hour + $0.007 per GB memory hour
- **Provisioned time**: $0.015 per vCPU hour + $0.0017 per GB memory hour
- **Build time**: $0.005 per build minute

**Example monthly cost** (assuming 8 hours active per day):
- 1 vCPU, 2GB memory: ~$25-35/month
- Includes automatic scaling, load balancing, and SSL

## 🚨 Troubleshooting

### Common Issues and Solutions

1. **Deployment Fails**
   ```bash
   # Check build logs in App Runner console
   # Verify Dockerfile builds locally:
   docker build -t test-app .
   docker run -p 8080:80 test-app
   ```

2. **Health Check Failures**
   ```bash
   # Test health endpoint locally:
   curl http://localhost:8080/health
   
   # Check App Runner logs for errors
   ```

3. **Database Connection Issues**
   ```bash
   # Verify Parameter Store values
   aws ssm get-parameter --name "/asset-management/production/connection-string" --with-decryption
   
   # Check RDS security groups allow App Runner access
   ```

4. **Environment Variable Issues**
   ```bash
   # Verify all required environment variables are set in App Runner
   # Check parameter ARNs are correct
   ```

## 🔐 Security Best Practices

1. **Use Parameter Store** for sensitive data
2. **Enable AWS WAF** for additional protection
3. **Configure VPC** if needed for database access
4. **Regular security updates** (automatic with App Runner)
5. **Monitor access logs** in CloudWatch

## 📈 Performance Optimization

1. **Enable response compression** in your application
2. **Use CloudFront** for static assets
3. **Optimize Docker image** size
4. **Configure appropriate instance** size
5. **Monitor and adjust** scaling settings

## 🔄 CI/CD Integration

App Runner automatically deploys when you push to your main branch. For more control:

1. **Use GitHub Actions** for pre-deployment testing
2. **Set up staging environment** for testing
3. **Configure deployment approvals** if needed

## ✅ Next Steps

1. **Run the parameter setup script** to store your secrets
2. **Create the App Runner service** using the console
3. **Test the deployment** using the health endpoint
4. **Set up monitoring alerts** in CloudWatch
5. **Configure custom domain** if needed

Your Asset Management System is now ready for serverless deployment with AWS App Runner! 🚀

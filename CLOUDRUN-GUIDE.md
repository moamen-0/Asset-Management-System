# Google Cloud Run Deployment Guide

## 🚀 Overview

Google Cloud Run is a fully managed serverless platform that automatically scales your containerized applications. It's perfect for your Asset Management System because it:

- **💰 Cost Effective**: Pay only when your app is serving requests
- **🔄 Auto Scaling**: Scales from 0 to thousands of instances automatically
- **🛡️ Secure**: Built-in security and compliance features
- **🚀 Fast Deployment**: Deploy in seconds with CI/CD
- **🌐 Global**: Deploy across multiple regions easily

## ✅ Prerequisites

1. **Google Cloud Account** with billing enabled
2. **Google Cloud CLI** installed and configured
3. **GitHub Repository** with your code
4. **Cloud SQL Database** (PostgreSQL/MySQL recommended)

## 🛠️ Setup Instructions

### Step 1: Initial Setup
Run the setup script to configure your Google Cloud project:

**PowerShell (Windows):**
```powershell
.\.gcp\setup-cloudrun.ps1
```

**Bash (Linux/Mac):**
```bash
chmod +x .gcp/setup-cloudrun.sh
./.gcp/setup-cloudrun.sh
```

### Step 2: Configure GitHub Secrets
Add these secrets to your GitHub repository (Settings → Secrets and variables → Actions):

- `GCP_PROJECT_ID`: Your Google Cloud Project ID
- `GCP_SA_KEY`: Service account key JSON (generated in step 1)

### Step 3: Update Configuration (if needed)
The deployment is pre-configured, but you can customize:

- **Service Name**: Edit `.github/workflows/deploy-cloudrun.yml`
- **Region**: Change `REGION` in the workflow file
- **Resources**: Add CPU/Memory limits in the Cloud Run service

## 🔧 Key Differences from AWS App Runner

| Feature | AWS App Runner | Google Cloud Run |
|---------|---------------|------------------|
| **Port** | 8080 | 8080 (same) |
| **Environment Variables** | Parameter Store | Secret Manager |
| **Configuration** | apprunner.yaml | GitHub Actions + gcloud |
| **Scaling** | Auto | Auto (more granular) |
| **Cold Starts** | ~2-3 seconds | ~1-2 seconds |
| **Max Request Timeout** | 120 seconds | 3600 seconds |

## 📊 Expected Performance

- **Cold Start**: 1-3 seconds
- **Concurrent Requests**: Up to 1000 per instance
- **Auto Scaling**: 0 to 1000+ instances
- **Request Timeout**: Up to 60 minutes
- **Memory**: 128MB to 32GB
- **CPU**: 0.08 to 8 vCPUs

## 🚀 Deployment Process

1. **Push to main branch** → Triggers GitHub Actions
2. **Build Docker image** → Optimized for Cloud Run
3. **Push to Artifact Registry** → Google's container registry
4. **Deploy to Cloud Run** → Automatic blue-green deployment
5. **Health Check** → Validates deployment success

## 🔍 Monitoring & Debugging

### Cloud Run Console
- **URL**: https://console.cloud.google.com/run
- View logs, metrics, and configuration

### Logs
```bash
# View recent logs
gcloud logs read --service=asset-management-system --limit=50

# Stream logs in real-time
gcloud logs tail --service=asset-management-system
```

### Health Check
Your service will be available at:
```
https://asset-management-system-<hash>-us-central1.a.run.app/health
```

## 💡 Benefits for Your Project

1. **Cost Savings**: Only pay when serving requests (vs. EC2 running 24/7)
2. **Zero Infrastructure**: No servers to manage
3. **Global Scale**: Deploy to multiple regions easily
4. **Security**: Built-in HTTPS, IAM integration
5. **Developer Experience**: Fast deployments, easy rollbacks

## 🔄 Migration from Current Setup

Your current AWS setup will work on Cloud Run with minimal changes:

1. ✅ **Docker**: Already containerized
2. ✅ **Environment Variables**: Already configured
3. ✅ **Health Checks**: Already implemented
4. ✅ **Database**: Compatible with Cloud SQL
5. 🔧 **Port**: Changed from 80 to 8080 (handled automatically)

## 📈 Scaling Configuration

Cloud Run auto-scales based on:
- **CPU utilization** (default: 60%)
- **Request concurrency** (default: 80 concurrent requests)
- **Custom metrics** (optional)

## 🛡️ Security Features

- **Automatic HTTPS**: SSL certificates managed automatically
- **IAM Integration**: Fine-grained access control
- **VPC Connector**: Connect to private resources
- **Secret Manager**: Secure environment variables
- **Container Security**: Automatic vulnerability scanning

## 💰 Cost Estimation

For a typical small-medium application:
- **Monthly cost**: $10-50 (depending on traffic)
- **Free tier**: 2 million requests/month
- **Pay-per-use**: Only when serving requests

## 🎯 Next Steps After Deployment

1. **Custom Domain**: Map your domain to Cloud Run
2. **CDN**: Enable Cloud CDN for better performance
3. **Monitoring**: Set up alerts and dashboards
4. **Backup**: Configure automated database backups
5. **Multi-region**: Deploy to multiple regions for HA

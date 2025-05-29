# Google Cloud Run Setup via Console (No CLI Required)

## 🌐 Quick Setup Checklist

### 1. **Google Cloud Console Setup**
- [ ] Go to [console.cloud.google.com](https://console.cloud.google.com)
- [ ] Create/Select project
- [ ] Enable billing (required for Cloud Run)

### 2. **Enable APIs** (Go to APIs & Services → Library)
- [ ] Cloud Run API
- [ ] Artifact Registry API  
- [ ] Secret Manager API
- [ ] Cloud Build API

### 3. **Create Secrets** (Go to Secret Manager)
Create these secrets with your actual values:

| Secret Name | Value Example |
|-------------|---------------|
| `db-connection-string` | `Server=your-db-server;Database=AssetManagementDB;User Id=admin;Password=your-password;Encrypt=True;TrustServerCertificate=True;` |
| `email-sender` | `your-email@gmail.com` |
| `email-account` | `your-email@gmail.com` |
| `email-password` | `your-app-password` |

### 4. **Create Cloud Run Service** (Go to Cloud Run)
1. Click **"Create Service"**
2. Select **"Continuously deploy from repository"**
3. Click **"Set up with Cloud Build"**

### 5. **Repository Configuration**
```
Source: GitHub
Repository: moamen-07/Asset-Management-System
Branch: main
Build Type: Dockerfile
Dockerfile Location: /Dockerfile.cloudrun
```

### 6. **Service Configuration**
```
Service Name: asset-management-system
Region: us-central1
CPU Allocation: Request-based
Ingress: All traffic
Authentication: Unauthenticated
```

### 7. **Environment Variables**
Add in Container → Environment Variables:
```
ASPNETCORE_ENVIRONMENT = Production
DOTNET_RUNNING_IN_CONTAINER = true
```

### 8. **Secret References**
Add in Security → Environment Variables from Secrets:
```
ConnectionStrings__DefaultConnection → db-connection-string:latest
Email__SenderEmail → email-sender:latest
Email__Account → email-account:latest
Email__Password → email-password:latest
```

### 9. **Deploy**
- [ ] Click **"Create"** 
- [ ] Wait for first deployment (5-10 minutes)
- [ ] Test your service URL

## 🔍 **After Deployment**

Your service will be available at:
```
https://asset-management-system-[random-hash]-us-central1.a.run.app
```

### Health Check
Test the deployment:
```
https://your-service-url/health
```

### View Logs
Go to Cloud Run → Your Service → Logs tab

## 📝 **Database Requirements**

You'll need a database accessible from Cloud Run:
- **Cloud SQL** (recommended)
- **External database** with public IP
- **Cloud SQL Proxy** for private connections

## 🎯 **Next Steps**
1. Set up custom domain (optional)
2. Configure CDN (optional)  
3. Set up monitoring and alerts
4. Configure auto-scaling settings

## 🚨 **Troubleshooting**
- Check logs in Cloud Run console
- Verify secrets are created correctly
- Ensure database is accessible
- Check Dockerfile.cloudrun builds successfully

# Deployment Monitoring Guide

## 🚀 Recent Changes (Latest Deployment)

### YAML Syntax Fixes Applied
- ✅ Fixed missing newlines between workflow steps
- ✅ Corrected indentation issues in deploy.yml
- ✅ Resolved "uses and run keys" parsing errors
- ✅ Ensured proper GitHub Actions YAML formatting

## 📊 How to Monitor the Deployment

### 1. GitHub Actions Workflow
- **URL**: `https://github.com/moamen-07/Asset-Management-System/actions`
- **Workflow**: "Deploy Asset Management System to AWS EC2"
- **Trigger**: Automatic on push to `main` branch

### 2. Key Deployment Steps to Watch
1. **Build and Test** - .NET compilation and testing
2. **Package Creation** - Application bundling
3. **.NET Runtime Installation** - Ensures .NET 9 is available on EC2
4. **Nginx Setup** - Web server configuration
5. **Application Deployment** - File extraction and service setup
6. **Health Checks** - Application accessibility verification

### 3. Expected Deployment Timeline
- **Total Duration**: ~10-15 minutes
- **Build Phase**: 3-5 minutes
- **Deployment Phase**: 5-8 minutes
- **Health Checks**: 2-3 minutes

## 🔍 Troubleshooting Commands

### If deployment fails, SSH into EC2 and run:

```bash
# Check application service status
sudo systemctl status assetmanagement

# View recent application logs
sudo journalctl -u assetmanagement --no-pager -l -n 50

# Check if .NET is installed
dotnet --version
dotnet --list-runtimes

# Check nginx status
sudo systemctl status nginx

# Test application directly
curl http://localhost:5000

# Test through nginx
curl http://localhost

# Check listening ports
sudo netstat -tlnp | grep -E ':80|:5000'

# Check application files
ls -la /var/www/assetmanagement/current/
```

### 4. Health Check Endpoints
- **Direct App**: `http://[EC2-IP]:5000`
- **Through Nginx**: `http://[EC2-IP]`
- **Nginx Health**: `http://[EC2-IP]/nginx-health`

## 🎯 Success Indicators

### GitHub Actions Success
- ✅ All workflow steps complete with green checkmarks
- ✅ Final step shows "🎉 DEPLOYMENT SUCCESSFUL!"
- ✅ No red error indicators in workflow

### Application Health
- ✅ Service status: `active (running)`
- ✅ Application responds on port 5000
- ✅ Nginx proxy works on port 80
- ✅ No error logs in recent journalctl output

## 🚨 Common Issues & Solutions

### Issue 1: .NET Runtime Missing
**Symptoms**: Service fails to start, "dotnet not found"
**Solution**: The workflow now auto-installs .NET 9 runtime

### Issue 2: Port Conflicts
**Symptoms**: "Address already in use"
**Solution**: Workflow includes service stop/restart logic

### Issue 3: Permission Issues
**Symptoms**: "Permission denied" errors
**Solution**: Proper ownership set in deployment step

### Issue 4: Database Connection
**Symptoms**: App starts but shows database errors
**Solution**: Check DB_SERVER and DB_PASSWORD secrets

## 📧 Post-Deployment Checklist

- [ ] GitHub Actions workflow completed successfully
- [ ] Application accessible via EC2 public IP
- [ ] Login functionality works
- [ ] File upload/download features operational
- [ ] Email notifications working (if configured)
- [ ] SSL certificate configured (if needed)

## 🔄 Rollback Procedure

If deployment fails and needs rollback:

```bash
# SSH into EC2
ssh -i your-key.pem ec2-user@[EC2-IP]

# Stop current service
sudo systemctl stop assetmanagement

# List available backups
ls -la /var/www/assetmanagement/backup_*

# Restore previous version (replace with actual backup name)
cd /var/www/assetmanagement
rm -rf current
mv backup_YYYYMMDD_HHMMSS current

# Restart service
sudo systemctl start assetmanagement
```

## 📞 Next Steps

1. **Monitor the GitHub Actions workflow** for completion
2. **Test the application** once deployment finishes
3. **Check logs** if any issues arise
4. **Verify all features** are working correctly

---
*Last Updated*: After YAML syntax fixes - Ready for deployment testing

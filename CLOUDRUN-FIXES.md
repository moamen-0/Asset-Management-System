# Cloud Run Deployment Troubleshooting Guide

## 🔧 **Fixed Issues:**

### ✅ **Port Configuration Fixed**
- **Problem**: App wasn't listening on Cloud Run's PORT environment variable
- **Solution**: Configured `builder.WebHost.UseUrls()` before building the app
- **Result**: App now properly listens on port 8080 (or PORT env var)

### ✅ **Email Configuration Fixed**
- **Problem**: Null reference errors in email service configuration
- **Solution**: Added null coalescing and default values
- **Result**: Email service won't crash on startup

### ✅ **Dockerfile Optimized**
- **Problem**: Hardcoded URLs conflicting with Cloud Run
- **Solution**: Removed hardcoded ASPNETCORE_URLS from Dockerfile
- **Result**: App respects Cloud Run's PORT environment variable

## 🚀 **Your Next Deployment Should Work**

After pushing the latest changes, your Cloud Run deployment should succeed. Here's what's been fixed:

### **Before (Broken):**
```csharp
// Port configured AFTER app.Build() - TOO LATE!
var app = builder.Build();
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}"); // This doesn't work
```

### **After (Fixed):**
```csharp
// Port configured BEFORE app.Build() - CORRECT!
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
var app = builder.Build();
```

## 🔍 **How to Monitor the Deployment:**

1. **Check Cloud Run Console**: Monitor the deployment progress
2. **View Logs**: Check logs for any remaining issues
3. **Test Health Endpoint**: Once deployed, test: `https://your-service-url/health`
4. **Test Login**: Access the main application: `https://your-service-url`

## 📊 **Expected Results:**

- ✅ **Build**: Should complete successfully 
- ✅ **Container Start**: Should start and listen on port 8080
- ✅ **Health Check**: `/health` endpoint should respond
- ✅ **Database**: Should connect to your AWS RDS instance
- ✅ **Email**: Should be configured but won't crash if not used

## 🚨 **If Still Having Issues:**

1. **Check the logs** in Cloud Run console
2. **Verify secrets** are created in Secret Manager
3. **Check database connectivity** from Cloud Run to your AWS RDS
4. **Ensure RDS security groups** allow connections from Google Cloud IPs

Your deployment should now work! 🎉

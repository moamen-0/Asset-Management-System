#!/bin/bash

# Asset Management System Deployment Troubleshooting Script
# This script helps diagnose common deployment issues

echo "🔍 Asset Management System Deployment Troubleshooting"
echo "================================================="

# Check .NET installation
echo ""
echo "1. .NET Runtime Check:"
echo "-----------------------"
if command -v dotnet &> /dev/null; then
    echo "✅ dotnet command found: $(which dotnet)"
    echo "Version: $(dotnet --version)"
    echo "Installed runtimes:"
    dotnet --list-runtimes | grep -E "(Microsoft.AspNetCore.App|Microsoft.NETCore.App)" || echo "❌ No runtimes found"
    
    # Check specifically for .NET 9
    if dotnet --list-runtimes | grep -q "Microsoft.AspNetCore.App 9.0"; then
        echo "✅ .NET 9 ASP.NET Core runtime found"
    else
        echo "❌ .NET 9 ASP.NET Core runtime NOT found"
    fi
else
    echo "❌ dotnet command not found"
    echo "Checking common locations:"
    ls -la /usr/local/bin/dotnet 2>/dev/null || echo "  - /usr/local/bin/dotnet: Not found"
    ls -la /home/ec2-user/.dotnet/dotnet 2>/dev/null || echo "  - /home/ec2-user/.dotnet/dotnet: Not found"
fi

# Check application files
echo ""
echo "2. Application Files Check:"
echo "---------------------------"
if [ -d "/var/www/assetmanagement/current" ]; then
    echo "✅ Application directory exists"
    echo "Files in application directory:"
    ls -la /var/www/assetmanagement/current/ | head -10
    
    if [ -f "/var/www/assetmanagement/current/AssetManagementSystem.PL.dll" ]; then
        echo "✅ Main application DLL found"
    else
        echo "❌ Main application DLL NOT found"
        echo "Available DLL files:"
        find /var/www/assetmanagement/current -name "*.dll" | head -5
    fi
    
    echo "Ownership and permissions:"
    ls -la /var/www/assetmanagement/current/AssetManagementSystem.PL.dll 2>/dev/null || echo "Cannot check DLL permissions"
else
    echo "❌ Application directory does not exist"
fi

# Check systemd service
echo ""
echo "3. Systemd Service Check:"
echo "-------------------------"
if [ -f "/etc/systemd/system/assetmanagement.service" ]; then
    echo "✅ Service file exists"
    echo "Service status:"
    sudo systemctl status assetmanagement --no-pager -l
    echo ""
    echo "Service is-enabled: $(sudo systemctl is-enabled assetmanagement 2>/dev/null || echo 'unknown')"
    echo "Service is-active: $(sudo systemctl is-active assetmanagement 2>/dev/null || echo 'unknown')"
else
    echo "❌ Service file does not exist"
fi

# Check recent logs
echo ""
echo "4. Recent Application Logs:"
echo "---------------------------"
echo "Last 20 lines from systemd journal:"
sudo journalctl -u assetmanagement -n 20 --no-pager 2>/dev/null || echo "No logs available"

# Check ports
echo ""
echo "5. Port Check:"
echo "--------------"
echo "Processes listening on port 5000:"
sudo netstat -tlnp | grep :5000 || echo "Nothing listening on port 5000"
echo ""
echo "Processes listening on port 80:"
sudo netstat -tlnp | grep :80 || echo "Nothing listening on port 80"

# Check nginx
echo ""
echo "6. Nginx Check:"
echo "---------------"
if command -v nginx &> /dev/null; then
    echo "✅ nginx found: $(which nginx)"
    echo "Nginx status:"
    sudo systemctl status nginx --no-pager -l 2>/dev/null || echo "Cannot get nginx status"
    echo ""
    echo "Nginx configuration test:"
    sudo nginx -t 2>&1 || echo "Nginx config test failed"
    
    if [ -f "/etc/nginx/conf.d/assetmanagement.conf" ]; then
        echo "✅ Asset Management nginx config exists"
    else
        echo "❌ Asset Management nginx config missing"
    fi
else
    echo "❌ nginx not found"
fi

# Quick connectivity test
echo ""
echo "7. Connectivity Test:"
echo "--------------------"
echo "Testing localhost:5000 (application):"
curl -I http://localhost:5000 2>/dev/null | head -1 || echo "❌ Cannot connect to application"

echo "Testing localhost:80 (nginx):"
curl -I http://localhost:80 2>/dev/null | head -1 || echo "❌ Cannot connect through nginx"

# Environment check
echo ""
echo "8. Environment Check:"
echo "--------------------"
echo "Current user: $(whoami)"
echo "PATH: $PATH"
echo "DOTNET_ROOT: ${DOTNET_ROOT:-'Not set'}"
echo "ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT:-'Not set'}"

# Disk space
echo ""
echo "9. System Resources:"
echo "-------------------"
echo "Disk usage for application directory:"
du -sh /var/www/assetmanagement 2>/dev/null || echo "Cannot check disk usage"
echo ""
echo "Available disk space:"
df -h / | tail -1

echo ""
echo "🔍 Troubleshooting complete!"
echo "If you're still having issues, check the logs above for error messages."

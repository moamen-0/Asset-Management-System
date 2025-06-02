# 🌐 Custom Domain Setup Guide: myosool.com → Google Cloud Run

## 📋 Overview

Connect your GoDaddy domain `myosool.com` to your Google Cloud Run service in the easiest way possible.

**Current URL**: https://asset-management-system-580642313220.us-central1.run.app/  
**Target URL**: https://myosool.com (or https://www.myosool.com)

---

## 🎯 **Option 1: Easy Setup (Recommended)**

### Step 1: Domain Mapping in Google Cloud Console

1. **Go to Cloud Run Console**
   - Open: https://console.cloud.google.com/run
   - Select your service: `asset-management-system`

2. **Add Custom Domain**
   - Click on your service name
   - Go to **"Manage Custom Domains"** tab
   - Click **"ADD MAPPING"**

3. **Configure Domain Mapping**
   ```
   Service: asset-management-system
   Domain: myosool.com
   Path: / (all paths)
   ```
   - Click **"CONTINUE"**

4. **Get DNS Records**
   Google will provide you with DNS records that look like:
   ```
   Type: A
   Name: @
   Value: 216.239.32.21
   
   Type: A  
   Name: @
   Value: 216.239.34.21
   
   Type: A
   Name: @
   Value: 216.239.36.21
   
   Type: A
   Name: @
   Value: 216.239.38.21
   
   Type: AAAA
   Name: @
   Value: 2001:4860:4802:32::15
   
   Type: AAAA
   Name: @
   Value: 2001:4860:4802:34::15
   
   Type: AAAA
   Name: @
   Value: 2001:4860:4802:36::15
   
   Type: AAAA
   Name: @
   Value: 2001:4860:4802:38::15
   ```

### Step 2: Update DNS in GoDaddy

1. **Login to GoDaddy**
   - Go to: https://dcc.godaddy.com/
   - Login with your account

2. **Access DNS Management**
   - Find your domain `myosool.com`
   - Click **"DNS"** or **"Manage DNS"**

3. **Delete Existing A Records** (if any)
   - Remove any existing A records for @ (root domain)
   - Remove any existing AAAA records for @ (root domain)

4. **Add New A Records**
   Add these 4 A records:
   ```
   Type: A, Host: @, Points to: 216.239.32.21, TTL: 1 Hour
   Type: A, Host: @, Points to: 216.239.34.21, TTL: 1 Hour  
   Type: A, Host: @, Points to: 216.239.36.21, TTL: 1 Hour
   Type: A, Host: @, Points to: 216.239.38.21, TTL: 1 Hour
   ```

5. **Add AAAA Records** (IPv6 - Optional but recommended)
   ```
   Type: AAAA, Host: @, Points to: 2001:4860:4802:32::15, TTL: 1 Hour
   Type: AAAA, Host: @, Points to: 2001:4860:4802:34::15, TTL: 1 Hour
   Type: AAAA, Host: @, Points to: 2001:4860:4802:36::15, TTL: 1 Hour
   Type: AAAA, Host: @, Points to: 2001:4860:4802:38::15, TTL: 1 Hour
   ```

6. **Add WWW Subdomain** (Optional)
   ```
   Type: CNAME, Host: www, Points to: ghs.googlehosted.com, TTL: 1 Hour
   ```

### Step 3: Verify Domain Ownership

1. **Back in Google Cloud Console**
   - Go to: https://console.cloud.google.com/run/domains
   - Click **"VERIFY DOMAIN"**
   - Follow the verification process (usually automatic)

2. **Wait for Propagation**
   - DNS changes take 5-60 minutes to propagate
   - SSL certificate provisioning takes 10-60 minutes

---

## 🎯 **Option 2: Using gcloud CLI**

If you prefer command line:

```bash
# Map domain to your service
gcloud run domain-mappings create \
    --service asset-management-system \
    --domain myosool.com \
    --region us-central1

# Get DNS records
gcloud run domain-mappings describe myosool.com \
    --region us-central1
```

---

## ✅ **Verification Steps**

### 1. Check DNS Propagation
```bash
# Check if DNS is propagated
nslookup myosool.com
dig myosool.com

# Should show Google's IP addresses
```

### 2. Test Your Domain
After 30-60 minutes, test:
- **HTTP**: http://myosool.com (should redirect to HTTPS)
- **HTTPS**: https://myosool.com (your app should load)
- **WWW**: https://www.myosool.com (if configured)

### 3. SSL Certificate
Google automatically provisions SSL certificates:
- Certificate is managed by Google
- Auto-renewal every 3 months
- No action required from you

---

## 🚨 **Troubleshooting**

### DNS Not Working?
1. **Check TTL**: Make sure TTL is set to 1 hour (3600 seconds)
2. **Clear DNS Cache**: 
   ```bash
   ipconfig /flushdns    # Windows
   sudo dscacheutil -flushcache  # Mac
   ```
3. **Wait Longer**: DNS can take up to 24 hours in some cases

### SSL Certificate Issues?
1. **Check Domain Mapping**: Ensure domain is properly mapped in Cloud Run
2. **Verify Ownership**: Complete domain verification in Google Cloud
3. **Check DNS**: A records must point to Google's servers

### Site Not Loading?
1. **Check Service**: Ensure your Cloud Run service is running
2. **Check Logs**: Review Cloud Run logs for errors
3. **Test Original URL**: Ensure https://asset-management-system-580642313220.us-central1.run.app/ still works

---

## 📊 **Expected Timeline**

| Step | Time |
|------|------|
| DNS Records Added | Immediate |
| DNS Propagation | 5-60 minutes |
| Domain Verification | 5-30 minutes |
| SSL Certificate | 10-60 minutes |
| **Total** | **30-90 minutes** |

---

## 🎯 **Final Configuration**

After setup, you'll have:

✅ **Primary Domain**: https://myosool.com  
✅ **WWW Domain**: https://www.myosool.com (optional)  
✅ **SSL Certificate**: Automatically managed by Google  
✅ **HTTP Redirect**: http:// automatically redirects to https://  
✅ **Old URL**: Still works as backup  

---

## 💡 **Pro Tips**

1. **Keep Old URL**: Don't delete the original Cloud Run URL as backup
2. **Monitor**: Set up uptime monitoring for your custom domain
3. **CDN**: Consider Cloud CDN for better global performance
4. **Multiple Domains**: You can map multiple domains to the same service
5. **Subdomains**: You can also map subdomains like `app.myosool.com`

---

## 🚀 **Next Steps After Domain Setup**

1. **Update Application URLs**: Update any hardcoded URLs in your app
2. **Update External Services**: Update any external services pointing to old URL
3. **SEO**: Set up 301 redirects if migrating from another domain
4. **Monitoring**: Set up monitoring and alerts for your new domain

---

**🎉 Your Asset Management System will be accessible at https://myosool.com within 1-2 hours!**

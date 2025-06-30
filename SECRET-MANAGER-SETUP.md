# Google Cloud Secret Manager Setup (Console Only)

## 🔐 Create Secrets in Google Cloud Console

### Step 1: Access Secret Manager
1. Go to [Google Cloud Console](https://console.cloud.google.com)
2. Search for "Secret Manager" in the top search bar
3. Click on **Secret Manager**
4. If prompted, click **"Enable API"**

### Step 2: Create Each Secret

Click **"+ CREATE SECRET"** for each of the following:

---

#### Secret 1: Database Connection String
```
Name: db-connection-string
Secret value: Server=34.71.143.39,1433;Database=AssetManagementDB;User Id=sqlserver;Password=asset@123!;Encrypt=True;TrustServerCertificate=True;
```
Click **"CREATE SECRET"**

---

#### Secret 2: Email Sender
```
Name: email-sender
Secret value: momenhassan7240@gmail.com
```
Click **"CREATE SECRET"**

---

#### Secret 3: Email Account
```
Name: email-account
Secret value: momenhassan7240@gmail.com
```
Click **"CREATE SECRET"**

---

#### Secret 4: Email Password
```
Name: email-password
Secret value: qebv vfcq ovof brjx
```
Click **"CREATE SECRET"**

---

## ✅ Verification

After creating all secrets, you should see 4 secrets in your Secret Manager:
- ✅ `db-connection-string`
- ✅ `email-sender`
- ✅ `email-account`
- ✅ `email-password`

## 🔧 Next Step: Cloud Run Service Configuration

When creating your Cloud Run service, add these environment variables:

### Regular Environment Variables:
```
ASPNETCORE_ENVIRONMENT = Production
DOTNET_RUNNING_IN_CONTAINER = true
PORT = 8080
```

### Secret References:
```
ConnectionStrings__DefaultConnection → db-connection-string:latest
Email__SenderEmail → email-sender:latest
Email__Account → email-account:latest
Email__Password → email-password:latest
```

## 📝 Important Notes:
- **Double Underscores**: Use `__` in `ConnectionStrings__DefaultConnection`
- **Version**: Always select `latest` for secret versions
- **Case Sensitive**: Environment variable names must match exactly

## 🎯 Ready for Cloud Run!
Once these secrets are created, you can proceed with creating your Cloud Run service and referencing these secrets as environment variables.

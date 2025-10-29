# App Registration Setup Guide

This guide shows you how to create an Azure AD App Registration with the minimum required permissions for a Microsoft Fabric workload.

## 🚀 Quick Setup

### 1. Create App Registration

1. Go to [Azure Portal](https://portal.azure.com) → **Azure Active Directory** → **App registrations**
2. Click **"New registration"**
3. Fill in:
   - **Name**: `{YourWorkloadName}` (e.g., "MyDataApp")
   - **Supported account types**: **Single tenant** (unless you need multi-tenant)
   - **Redirect URI**: Leave empty for now
4. Click **"Register"**

### 2. Configure Authentication

1. Go to **Authentication** tab
2. Add **Redirect URIs**:
   ```
   https://msit.fabric.microsoft.com/workloadSignIn/{YOUR_TENANT_ID}/{YOUR_ORG}.{YOUR_WORKLOAD}
   https://msit.powerbi.com/workloadSignIn/{YOUR_TENANT_ID}/{YOUR_ORG}.{YOUR_WORKLOAD}
   https://app.fabric.microsoft.com/workloadSignIn/{YOUR_TENANT_ID}/{YOUR_ORG}.{YOUR_WORKLOAD}
   https://app.powerbi.com/workloadSignIn/{YOUR_TENANT_ID}/{YOUR_ORG}.{YOUR_WORKLOAD}
   http://localhost:60006/close
   ```
3. Under **Implicit grant and hybrid flows**:
   - ✅ **Access tokens**
   - ✅ **ID tokens**
4. Click **"Save"**

### 3. Add Required Permissions

Go to **API permissions** tab and add these **ESSENTIAL** permissions:

#### Microsoft Graph (5 permissions)
- ✅ **openid** (Delegated) - Sign users in
- ✅ **profile** (Delegated) - View users' basic profile  
- ✅ **offline_access** (Delegated) - Maintain access to data
- ✅ **User.Read** (Delegated) - Sign in and read user profile
- ✅ **User.ReadBasic.All** (Delegated) - Read all users' basic profiles

#### Power BI Service (4 permissions)
- ✅ **Fabric.Extend** (Delegated) - Extend Fabric with new item types
- ✅ **Item.Read.All** (Delegated) - Read all Fabric items
- ✅ **Workspace.Read.All** (Delegated) - View all workspaces
- ✅ **Capacity.Read.All** (Delegated) - View all capacities

### 4. Grant Admin Consent

1. Click **"Grant admin consent for {Your Organization}"**
2. Confirm the consent

### 5. Create Client Secret

1. Go to **Certificates & secrets** tab
2. Click **"New client secret"**
3. Add description: "Workload Secret"
4. Set expiration: **24 months** (recommended)
5. Click **"Add"**
6. **⚠️ COPY THE SECRET VALUE** - you won't see it again!

## 📋 What You Need

After setup, you'll have:
- **Application (client) ID** - Use as `clientId` in config
- **Client secret value** - Use as `clientSecret` in config  
- **Directory (tenant) ID** - Use as `tenantId` in config

## 🔧 Update Your Config

Add these to your `workload-config.json`:

```json
{
  "azure": {
    "tenantId": "your-tenant-id-here"
  },
  "environments": {
    "dev": {
      "clientId": "your-app-id-here",
      "clientSecret": "your-secret-value-here"
    }
  }
}
```

## ✅ Essential Permissions Summary

**Minimum required for Fabric workload:**
- Microsoft Graph: `openid`, `profile`, `offline_access`, `User.Read`, `User.ReadBasic.All`
- Power BI Service: `Fabric.Extend`, `Item.Read.All`, `Workspace.Read.All`, `Capacity.Read.All`

**Total: 9 permissions** (all essential for full functionality)

---

**Next Step**: Run `./setup-workload-simple.ps1` to configure your workload! 🚀

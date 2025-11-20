# Secrets Configuration Setup

## Overview
This project uses **dotnet user-secrets** for local development to keep sensitive configuration out of Git.

User Secrets ID: `638e5409-d8cb-4377-afe8-d71cf183c829`

## First-Time Setup

### 1. Configure Azure AD Secrets
Run these commands in the `api/dotnet/GomokuApi` directory:

```bash
# Azure AD Configuration
dotnet user-secrets set "AzureAd:Instance" "https://login.microsoftonline.com/"
dotnet user-secrets set "AzureAd:Domain" "YOUR_TENANT.onmicrosoft.com"
dotnet user-secrets set "AzureAd:TenantId" "YOUR_TENANT_ID"
dotnet user-secrets set "AzureAd:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "AzureAd:ClientSecret" "YOUR_CLIENT_SECRET"
```

### 2. Configure Database Connection
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=GomokuAuth;User Id=sa;Password=YOUR_SA_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### 3. Verify Configuration
```bash
dotnet user-secrets list
```

## Where Secrets Are Stored
Secrets are stored locally at:
- **macOS/Linux**: `~/.microsoft/usersecrets/638e5409-d8cb-4377-afe8-d71cf183c829/secrets.json`
- **Windows**: `%APPDATA%\Microsoft\UserSecrets\638e5409-d8cb-4377-afe8-d71cf183c829\secrets.json`

## Configuration Priority
.NET loads configuration in this order (later overrides earlier):
1. `appsettings.json` (checked into Git - contains placeholders)
2. `appsettings.Development.json` (local file - NOT in Git)
3. User Secrets (for Development environment only)
4. Environment variables
5. Command-line arguments

## Recommended Approach
Choose **ONE** of these methods for local development:

### Option A: User Secrets (Recommended)
- Use dotnet user-secrets commands above
- Secrets stored outside project directory
- Automatic with `dotnet run`

### Option B: appsettings.Development.json
1. Copy `appsettings.json` to `appsettings.Development.json`
2. Replace PLACEHOLDER values with real credentials
3. This file is already in `.gitignore` and will NOT be committed

## Production Deployment
For production, use:
- **Azure Key Vault** (recommended for Azure deployments)
- **Environment Variables** (for Docker/container deployments)
- **Azure App Service Configuration** (Application Settings)

Never commit `appsettings.Production.json` to Git.

## Secret Rotation (If Compromised)
If secrets were accidentally committed to Git:

1. **Rotate all secrets immediately**:
   - Azure Portal → App registrations → Your app → Certificates & secrets → New client secret
   - Update database password (if exposed)

2. **Update local configuration** with new secrets using commands above

3. **Clean Git history** (if secrets were committed):
   ```bash
   # WARNING: This rewrites Git history
   git filter-branch --force --index-filter \
     "git rm --cached --ignore-unmatch api/dotnet/GomokuApi/appsettings.Development.json" \
     --prune-empty --tag-name-filter cat -- --all
   
   # Force push (coordinate with team first!)
   git push origin --force --all
   ```

## Team Onboarding
Share these instructions with new developers:
1. Clone the repository
2. Follow "First-Time Setup" above
3. Run `dotnet run` - secrets are automatically loaded

## Troubleshooting

### "Configuration value not found"
- Run `dotnet user-secrets list` to verify secrets are set
- Check you're in Development environment: `echo $ASPNETCORE_ENVIRONMENT`
- Verify user secrets ID matches in `GomokuApi.csproj`: `<UserSecretsId>638e5409-d8cb-4377-afe8-d71cf183c829</UserSecretsId>`

### "Cannot connect to database"
- Verify Docker SQL Server is running: `docker ps`
- Test connection string manually
- Check firewall/network settings

### Need to clear all secrets
```bash
dotnet user-secrets clear
```

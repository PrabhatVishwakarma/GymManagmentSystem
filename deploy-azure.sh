#!/bin/bash

# ================================================================
# Gym Management System - Azure Deployment Script
# This script deploys the application to Azure
# ================================================================

echo ""
echo "================================================"
echo "   Azure Deployment - Gym Management System"
echo "================================================"
echo ""

# Configuration Variables (Update these for your setup)
RESOURCE_GROUP="GymManagementRG"
LOCATION="eastus"
SQL_SERVER_NAME="gymmgmt-sql-$(date +%s)"  # Unique name with timestamp
SQL_ADMIN_USER="gymadmin"
SQL_ADMIN_PASSWORD="GymSecure@123!"  # Change this!
SQL_DB_NAME="GymManagementDB"
APP_SERVICE_PLAN="GymManagementPlan"
API_APP_NAME="gymmgmt-api-$(date +%s)"
FRONTEND_STORAGE_ACCOUNT="gymmgmtui$(date +%s)"
FRONTEND_CONTAINER="\$web"

echo "Configuration:"
echo "  Resource Group: $RESOURCE_GROUP"
echo "  Location: $LOCATION"
echo "  SQL Server: $SQL_SERVER_NAME"
echo "  Database: $SQL_DB_NAME"
echo "  API App: $API_APP_NAME"
echo ""

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
    echo "❌ Azure CLI is not installed!"
    echo "Install from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
    exit 1
fi

# Login to Azure
echo "[1/12] Logging into Azure..."
az login

# Create Resource Group
echo ""
echo "[2/12] Creating Resource Group..."
az group create \
    --name $RESOURCE_GROUP \
    --location $LOCATION

# Create SQL Server
echo ""
echo "[3/12] Creating SQL Server..."
az sql server create \
    --name $SQL_SERVER_NAME \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --admin-user $SQL_ADMIN_USER \
    --admin-password $SQL_ADMIN_PASSWORD

# Configure Firewall (Allow Azure Services)
echo ""
echo "[4/12] Configuring SQL Server Firewall..."
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name AllowAzureServices \
    --start-ip-address 0.0.0.0 \
    --end-ip-address 0.0.0.0

# Optional: Allow your current IP for testing
MY_IP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name AllowMyIP \
    --start-ip-address $MY_IP \
    --end-ip-address $MY_IP

# Create SQL Database
echo ""
echo "[5/12] Creating SQL Database..."
az sql db create \
    --resource-group $RESOURCE_GROUP \
    --server $SQL_SERVER_NAME \
    --name $SQL_DB_NAME \
    --service-objective S0 \
    --zone-redundant false

# Create App Service Plan
echo ""
echo "[6/12] Creating App Service Plan..."
az appservice plan create \
    --name $APP_SERVICE_PLAN \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --sku B1 \
    --is-linux

# Create Web App for API
echo ""
echo "[7/12] Creating Web App for Backend API..."
az webapp create \
    --resource-group $RESOURCE_GROUP \
    --plan $APP_SERVICE_PLAN \
    --name $API_APP_NAME \
    --runtime "DOTNETCORE:8.0"

# Build Backend
echo ""
echo "[8/12] Building Backend for Linux..."
cd GymManagmentSystem
dotnet publish -c Release -r linux-x64 --self-contained false -o ./publish

# Create deployment package
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy Backend to Azure
echo ""
echo "[9/12] Deploying Backend to Azure..."
az webapp deploy \
    --resource-group $RESOURCE_GROUP \
    --name $API_APP_NAME \
    --src-path deploy.zip \
    --type zip

# Configure App Settings
echo ""
echo "[10/12] Configuring Application Settings..."

# Build connection string
CONNECTION_STRING="Server=tcp:${SQL_SERVER_NAME}.database.windows.net,1433;Initial Catalog=${SQL_DB_NAME};Persist Security Info=False;User ID=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Generate JWT Secret (32 character random string)
JWT_SECRET=$(openssl rand -base64 32)

az webapp config connection-string set \
    --resource-group $RESOURCE_GROUP \
    --name $API_APP_NAME \
    --connection-string-type SQLAzure \
    --settings DefaultConnection="$CONNECTION_STRING"

az webapp config appsettings set \
    --resource-group $RESOURCE_GROUP \
    --name $API_APP_NAME \
    --settings \
        JWT__Secret="$JWT_SECRET" \
        JWT__ValidIssuer="GymManagementSystem" \
        JWT__ValidAudience="GymManagementSystem" \
        ASPNETCORE_ENVIRONMENT="Production"

# Create Storage Account for Frontend
echo ""
echo "[11/12] Creating Storage Account for Frontend..."
az storage account create \
    --name $FRONTEND_STORAGE_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --location $LOCATION \
    --sku Standard_LRS \
    --kind StorageV2 \
    --access-tier Hot

# Enable Static Website Hosting
az storage blob service-properties update \
    --account-name $FRONTEND_STORAGE_ACCOUNT \
    --static-website \
    --index-document index.html \
    --404-document index.html

# Get Storage Account Key
STORAGE_KEY=$(az storage account keys list \
    --resource-group $RESOURCE_GROUP \
    --account-name $FRONTEND_STORAGE_ACCOUNT \
    --query '[0].value' -o tsv)

# Build and Deploy Frontend
echo ""
echo "[12/12] Building and Deploying Frontend..."
cd ../gym-management-ui

# Update API endpoint in frontend
API_URL="https://${API_APP_NAME}.azurewebsites.net"
echo "REACT_APP_API_URL=$API_URL" > .env.production

npm install
npm run build

# Upload to Azure Storage
az storage blob upload-batch \
    --account-name $FRONTEND_STORAGE_ACCOUNT \
    --account-key $STORAGE_KEY \
    --destination '$web' \
    --source ./build \
    --overwrite

# Get Frontend URL
FRONTEND_URL=$(az storage account show \
    --name $FRONTEND_STORAGE_ACCOUNT \
    --resource-group $RESOURCE_GROUP \
    --query "primaryEndpoints.web" -o tsv)

# Update CORS on API to allow Frontend
echo ""
echo "Updating CORS settings..."
az webapp cors add \
    --resource-group $RESOURCE_GROUP \
    --name $API_APP_NAME \
    --allowed-origins "$FRONTEND_URL"

echo ""
echo "================================================"
echo "   DEPLOYMENT COMPLETED SUCCESSFULLY! ✅"
echo "================================================"
echo ""
echo "Your Gym Management System is now live!"
echo ""
echo "📊 Backend API URL:"
echo "   $API_URL"
echo ""
echo "🌐 Frontend URL:"
echo "   $FRONTEND_URL"
echo ""
echo "🗄️  Database:"
echo "   Server: $SQL_SERVER_NAME.database.windows.net"
echo "   Database: $SQL_DB_NAME"
echo "   Admin User: $SQL_ADMIN_USER"
echo ""
echo "🔐 Important Security Info:"
echo "   JWT Secret: $JWT_SECRET"
echo "   SQL Password: $SQL_ADMIN_PASSWORD"
echo ""
echo "⚠️  IMPORTANT: Save these credentials securely!"
echo ""
echo "Next Steps:"
echo "1. Visit $FRONTEND_URL"
echo "2. Database migrations will run automatically on first start"
echo "3. Default admin login:"
echo "   Email: admin@gym.com"
echo "   Password: Admin@123"
echo ""
echo "4. Update CORS settings in Azure Portal if needed"
echo "5. Configure custom domain (optional)"
echo "6. Set up monitoring and alerts"
echo ""
echo "📖 Documentation:"
echo "   Azure Portal: https://portal.azure.com"
echo "   Resource Group: $RESOURCE_GROUP"
echo ""
echo "💰 Estimated Monthly Cost:"
echo "   - App Service (B1): ~$13/month"
echo "   - SQL Database (S0): ~$15/month"
echo "   - Storage Account: ~$1/month"
echo "   Total: ~$29/month"
echo ""
echo "To scale for production, consider upgrading to:"
echo "   - App Service: P1V2 (~$80/month)"
echo "   - SQL Database: S1 (~$30/month)"
echo ""


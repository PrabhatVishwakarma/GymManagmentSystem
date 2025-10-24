#!/bin/bash

# ================================================================
# Gym Management System - Production Build Script (Linux/Mac)
# This script builds both backend and frontend for production
# ================================================================

echo ""
echo "================================================"
echo "   Gym Management System - Production Build"
echo "================================================"
echo ""

# Set build configuration
BUILD_CONFIG="Release"
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
OUTPUT_DIR="$SCRIPT_DIR/ProductionBuild"
BACKEND_DIR="$SCRIPT_DIR/GymManagmentSystem"
FRONTEND_DIR="$SCRIPT_DIR/gym-management-ui"

# Clean previous build
echo "[1/6] Cleaning previous build..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR/Backend/Linux"
mkdir -p "$OUTPUT_DIR/Backend/Windows"
mkdir -p "$OUTPUT_DIR/Frontend"
mkdir -p "$OUTPUT_DIR/Documentation"

# Build Backend (Linux x64)
echo ""
echo "[2/6] Building Backend API for Linux..."
cd "$BACKEND_DIR"
dotnet publish -c $BUILD_CONFIG -r linux-x64 --self-contained true -p:PublishSingleFile=false -o "$OUTPUT_DIR/Backend/Linux"

# Build Backend (Windows x64) - for customers who want Windows deployment
echo ""
echo "[3/6] Building Backend API for Windows..."
dotnet publish -c $BUILD_CONFIG -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o "$OUTPUT_DIR/Backend/Windows"

# Build Frontend
echo ""
echo "[4/6] Building Frontend..."
cd "$FRONTEND_DIR"
npm install
npm run build
cp -r build/* "$OUTPUT_DIR/Frontend/"

# Copy Documentation
echo ""
echo "[5/6] Copying documentation..."
cd "$SCRIPT_DIR"
cp README.md "$OUTPUT_DIR/Documentation/" 2>/dev/null || true
cp DEPLOYMENT_GUIDE.md "$OUTPUT_DIR/Documentation/" 2>/dev/null || true
cp FEATURES_IMPLEMENTED.md "$OUTPUT_DIR/Documentation/" 2>/dev/null || true
cp CONFIGURATION.md "$OUTPUT_DIR/Documentation/" 2>/dev/null || true

# Create README for distribution
echo ""
echo "[6/6] Creating distribution README..."
cat > "$OUTPUT_DIR/README.txt" << 'EOF'
# Gym Management System - Production Build

## Contents
- Backend/Linux/ - Linux binaries for cloud deployment
- Backend/Windows/ - Windows executable (self-contained)
- Frontend/ - Production-optimized React build
- Documentation/ - System documentation

## Quick Start

### For Linux/Cloud Deployment:
1. Navigate to Backend/Linux/
2. Set environment variables or edit appsettings.json
3. chmod +x GymManagmentSystem
4. ./GymManagmentSystem

### For Windows Deployment:
1. Navigate to Backend/Windows/
2. Edit appsettings.json with your database connection
3. Run GymManagmentSystem.exe

### Frontend Deployment:
- Upload Frontend/ contents to your web server or static hosting
- Configure API endpoint in the frontend build

### Configuration Required:
- Database: SQL Server connection string
- JWT: Secret key for authentication
- CORS: Allowed origins for frontend

See DEPLOYMENT_GUIDE.md for detailed instructions.

## Support
For support, contact: support@yourdomain.com

EOF

echo "Build Date: $(date)" >> "$OUTPUT_DIR/README.txt"

# Create configuration template
echo ""
echo "Creating configuration template..."
cat > "$OUTPUT_DIR/Backend/Linux/appsettings.Production.template.json" << 'EOF'
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=GymManagementDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "JWT": {
    "ValidAudience": "GymManagementSystem",
    "ValidIssuer": "GymManagementSystem",
    "Secret": "CHANGE_THIS_TO_A_SECURE_KEY_AT_LEAST_32_CHARACTERS"
  },
  "CORS": {
    "AllowedOrigins": [
      "https://yourdomain.com",
      "https://www.yourdomain.com"
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
EOF

# Make Linux binary executable
chmod +x "$OUTPUT_DIR/Backend/Linux/GymManagmentSystem"

# Create compressed archive
echo ""
echo "Creating TAR.GZ archive..."
cd "$SCRIPT_DIR"
tar -czf "ProductionBuild.tar.gz" -C "$OUTPUT_DIR" .

echo ""
echo "================================================"
echo "   BUILD COMPLETED SUCCESSFULLY!"
echo "================================================"
echo ""
echo "Output Location: $OUTPUT_DIR"
echo "Archive Created: ProductionBuild.tar.gz"
echo ""
echo "Next Steps:"
echo "1. Test the build locally"
echo "2. Deploy to your hosting environment"
echo "3. Configure appsettings.json with production values"
echo "4. Run database migrations"
echo ""


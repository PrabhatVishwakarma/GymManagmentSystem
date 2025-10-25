# Gym Management System - Windows Deployment Script
# This script creates a complete deployment package

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Gym Management System - Deployment" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Create distribution directory
$DistPath = ".\Distribution"
if (Test-Path $DistPath) {
    Write-Host "Cleaning existing distribution folder..." -ForegroundColor Yellow
    Remove-Item -Path $DistPath -Recurse -Force
}
New-Item -ItemType Directory -Path $DistPath -Force | Out-Null
New-Item -ItemType Directory -Path "$DistPath\Backend" -Force | Out-Null
New-Item -ItemType Directory -Path "$DistPath\Frontend" -Force | Out-Null

# Build Backend
Write-Host "Building Backend (Self-Contained)..." -ForegroundColor Green
Set-Location -Path ".\GymManagmentSystem"
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "..\$DistPath\Backend"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Backend build failed!" -ForegroundColor Red
    Set-Location ..
    exit 1
}

Set-Location ..
Write-Host "✓ Backend built successfully" -ForegroundColor Green

# Build Frontend
Write-Host ""
Write-Host "Building Frontend..." -ForegroundColor Green
Set-Location -Path ".\gym-management-ui"

# Check if node_modules exists
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing frontend dependencies..." -ForegroundColor Yellow
    npm install
}

npm run build

if ($LASTEXITCODE -ne 0) {
    Write-Host "Frontend build failed!" -ForegroundColor Red
    Set-Location ..
    exit 1
}

# Copy frontend build
Write-Host "Copying Frontend files..." -ForegroundColor Yellow
Copy-Item -Path ".\build\*" -Destination "..\$DistPath\Frontend" -Recurse -Force

Set-Location ..
Write-Host "✓ Frontend built successfully" -ForegroundColor Green

# Create installation instructions
Write-Host ""
Write-Host "Creating installation instructions..." -ForegroundColor Yellow

$InstallInstructions = @"
====================================================
  GYM MANAGEMENT SYSTEM - INSTALLATION GUIDE
====================================================

SYSTEM REQUIREMENTS:
--------------------
- Windows 10/11 (64-bit)
- 4GB RAM minimum
- 500MB free disk space
- Internet connection for MongoDB Atlas

INSTALLATION STEPS:
-------------------

1. BACKEND SETUP:
   
   a) Navigate to the 'Backend' folder
   
   b) IMPORTANT - Configure appsettings.json:
      - Open appsettings.json in a text editor
      - Update MongoDB connection string with your credentials
      - Update JWT secret key (keep it secure!)
      - Configure Email settings if needed
   
   c) Run the application:
      - Double-click 'GymManagmentSystem.exe'
      - OR open Command Prompt and run: GymManagmentSystem.exe
      - The API will start on: http://localhost:5000
   
   d) Keep this window open while using the application

2. FRONTEND SETUP:
   
   a) Install Node.js (if not installed):
      - Download from: https://nodejs.org/
      - Choose LTS version
      - Install with default settings
   
   b) Navigate to the 'Frontend' folder
   
   c) Open Command Prompt in this folder and run:
      npm install -g serve
      serve -s . -l 3000
   
   d) Open your browser and go to: http://localhost:3000

3. FIRST TIME LOGIN:
   
   Default Admin Credentials:
   - Email: admin@gym.com
   - Password: Admin@123
   
   IMPORTANT: Change the admin password after first login!

TROUBLESHOOTING:
----------------

Problem: "Port already in use"
Solution: Another application is using port 5000 or 3000
         Close other applications or use different ports

Problem: "Cannot connect to MongoDB"
Solution: Check your internet connection and MongoDB connection string
         Verify IP whitelist in MongoDB Atlas includes your IP

Problem: Frontend shows "Network Error"
Solution: Ensure backend is running on http://localhost:5000
         Check CORS settings in appsettings.json

Problem: "This app can't run on your PC" error
Solution: You may need the self-contained version for your system
         Or install .NET 8.0 Runtime from Microsoft

FEATURES:
---------
✓ User Management (Admin, Staff, Member roles)
✓ Enquiry Management (Track leads)
✓ Membership Plans
✓ Member Registrations
✓ Payment Processing
✓ Receipt Generation
✓ Reports & Analytics
✓ Activity Logging
✓ Email Notifications

CONFIGURATION:
--------------

MongoDB Atlas Setup:
1. Create account at https://cloud.mongodb.com
2. Create a cluster (Free tier available)
3. Add database user in "Database Access"
4. Whitelist your IP in "Network Access"
5. Get connection string and update appsettings.json

Email Configuration (Optional):
- Uses Gmail SMTP by default
- Create App Password in Gmail settings
- Update credentials in appsettings.json

SECURITY NOTES:
---------------
⚠ Do not share appsettings.json with sensitive data
⚠ Use strong passwords for admin accounts
⚠ Keep JWT secret key confidential
⚠ Regularly backup your MongoDB database
⚠ Use HTTPS in production environments

SUPPORT:
--------
For issues or questions:
- Check the README.md file
- Review MongoDB Atlas documentation
- Ensure all requirements are met

====================================================
              Thank you for using our system!
====================================================
"@

$InstallInstructions | Out-File -FilePath "$DistPath\INSTALLATION-INSTRUCTIONS.txt" -Encoding UTF8

# Create a quick start script
$QuickStart = @"
@echo off
echo Starting Gym Management System Backend...
echo.
echo API will be available at: http://localhost:5000
echo Keep this window open while using the application
echo.
echo Press Ctrl+C to stop the server
echo.
cd Backend
GymManagmentSystem.exe
pause
"@

$QuickStart | Out-File -FilePath "$DistPath\START-BACKEND.bat" -Encoding ASCII

$FrontendStart = @"
@echo off
echo Starting Gym Management System Frontend...
echo.
echo Application will be available at: http://localhost:3000
echo Your browser should open automatically
echo.
echo Keep this window open while using the application
echo Press Ctrl+C to stop the server
echo.
cd Frontend
serve -s . -l 3000
pause
"@

$FrontendStart | Out-File -FilePath "$DistPath\START-FRONTEND.bat" -Encoding ASCII

# Create README
$Readme = @"
# Gym Management System - Distribution Package

This package contains a complete, ready-to-deploy version of the Gym Management System.

## Quick Start

1. Read `INSTALLATION-INSTRUCTIONS.txt` first
2. Configure `Backend\appsettings.json` with your MongoDB credentials
3. Run `START-BACKEND.bat` to start the API server
4. Run `START-FRONTEND.bat` to start the web interface
5. Open browser to http://localhost:3000

## Package Contents

- **Backend**: .NET 8 API (Self-contained, no .NET installation required)
- **Frontend**: React web application (optimized production build)
- **Documentation**: Installation and configuration guides

## Version Information

- Build Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
- Backend: .NET 8.0
- Frontend: React 18
- Database: MongoDB Atlas

For detailed instructions, see INSTALLATION-INSTRUCTIONS.txt
"@

$Readme | Out-File -FilePath "$DistPath\README.txt" -Encoding UTF8

# Create ZIP archive
Write-Host ""
Write-Host "Creating distribution archive..." -ForegroundColor Yellow
$ZipName = "GymManagementSystem-v1.0-$(Get-Date -Format 'yyyyMMdd').zip"
Compress-Archive -Path "$DistPath\*" -DestinationPath $ZipName -Force

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "✓ DEPLOYMENT COMPLETE!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Distribution folder: $DistPath" -ForegroundColor Cyan
Write-Host "ZIP archive: $ZipName" -ForegroundColor Cyan
Write-Host ""
Write-Host "Package Size:" -ForegroundColor Yellow
Get-ChildItem -Path $DistPath -Recurse | Measure-Object -Property Length -Sum | ForEach-Object {
    Write-Host "  $([math]::Round($_.Sum / 1MB, 2)) MB" -ForegroundColor White
}
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Copy '$ZipName' to target computer" -ForegroundColor White
Write-Host "2. Extract all files" -ForegroundColor White
Write-Host "3. Follow INSTALLATION-INSTRUCTIONS.txt" -ForegroundColor White
Write-Host ""
Write-Host "IMPORTANT: Update appsettings.json with production credentials!" -ForegroundColor Red
Write-Host ""


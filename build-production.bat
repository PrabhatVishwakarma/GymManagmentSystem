@echo off
REM ================================================================
REM Gym Management System - Production Build Script
REM This script builds both backend and frontend for production
REM ================================================================

echo.
echo ================================================
echo   Gym Management System - Production Build
echo ================================================
echo.

REM Set build configuration
set BUILD_CONFIG=Release
set OUTPUT_DIR=%~dp0\ProductionBuild
set BACKEND_DIR=%~dp0\GymManagmentSystem
set FRONTEND_DIR=%~dp0\gym-management-ui

REM Clean previous build
echo [1/6] Cleaning previous build...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%\Backend"
mkdir "%OUTPUT_DIR%\Frontend"
mkdir "%OUTPUT_DIR%\Documentation"

REM Build Backend (Windows x64)
echo.
echo [2/6] Building Backend API...
cd "%BACKEND_DIR%"
dotnet publish -c %BUILD_CONFIG% -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o "%OUTPUT_DIR%\Backend\Windows"

REM Build Backend (Linux x64) - for Azure/AWS deployment
echo.
echo [3/6] Building Backend API for Linux...
dotnet publish -c %BUILD_CONFIG% -r linux-x64 --self-contained true -p:PublishSingleFile=false -o "%OUTPUT_DIR%\Backend\Linux"

REM Build Frontend
echo.
echo [4/6] Building Frontend...
cd "%FRONTEND_DIR%"
call npm install
call npm run build
xcopy /E /I /Y "build" "%OUTPUT_DIR%\Frontend"

REM Copy Documentation
echo.
echo [5/6] Copying documentation...
cd "%~dp0"
copy "README.md" "%OUTPUT_DIR%\Documentation\" >nul 2>&1
copy "DEPLOYMENT_GUIDE.md" "%OUTPUT_DIR%\Documentation\" >nul 2>&1
copy "FEATURES_IMPLEMENTED.md" "%OUTPUT_DIR%\Documentation\" >nul 2>&1
copy "CONFIGURATION.md" "%OUTPUT_DIR%\Documentation\" >nul 2>&1

REM Create README for distribution
echo.
echo [6/6] Creating distribution README...
(
echo # Gym Management System - Production Build
echo.
echo ## Contents
echo - Backend/Windows/ - Windows executable ^(self-contained^)
echo - Backend/Linux/ - Linux binaries for cloud deployment
echo - Frontend/ - Production-optimized React build
echo - Documentation/ - System documentation
echo.
echo ## Quick Start
echo.
echo ### For Windows Deployment:
echo 1. Navigate to Backend/Windows/
echo 2. Edit appsettings.json with your database connection
echo 3. Run GymManagmentSystem.exe
echo.
echo ### For Cloud Deployment ^(Azure/AWS^):
echo 1. Use Backend/Linux/ for Linux servers
echo 2. Upload Frontend/ to static hosting ^(Azure Blob Storage, S3, etc.^)
echo 3. Configure environment variables for connection strings
echo.
echo ### Configuration Required:
echo - Database: SQL Server connection string
echo - JWT: Secret key for authentication
echo - CORS: Allowed origins for frontend
echo.
echo See DEPLOYMENT_GUIDE.md for detailed instructions.
echo.
echo ## Support
echo For support, contact: support@yourdomain.com
echo.
echo Build Date: %date% %time%
) > "%OUTPUT_DIR%\README.txt"

REM Create configuration template
echo.
echo Creating configuration template...
(
echo {
echo   "ConnectionStrings": {
echo     "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=GymManagementDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
echo   },
echo   "JWT": {
echo     "ValidAudience": "GymManagementSystem",
echo     "ValidIssuer": "GymManagementSystem",
echo     "Secret": "CHANGE_THIS_TO_A_SECURE_KEY_AT_LEAST_32_CHARACTERS"
echo   },
echo   "CORS": {
echo     "AllowedOrigins": [
echo       "https://yourdomain.com",
echo       "https://www.yourdomain.com"
echo     ]
echo   },
echo   "Logging": {
echo     "LogLevel": {
echo       "Default": "Information",
echo       "Microsoft.AspNetCore": "Warning"
echo     }
echo   },
echo   "AllowedHosts": "*"
echo }
) > "%OUTPUT_DIR%\Backend\Windows\appsettings.Production.template.json"

REM Create compressed archive
echo.
echo Creating ZIP archive...
powershell Compress-Archive -Path "%OUTPUT_DIR%\*" -DestinationPath "%OUTPUT_DIR%.zip" -Force

echo.
echo ================================================
echo   BUILD COMPLETED SUCCESSFULLY!
echo ================================================
echo.
echo Output Location: %OUTPUT_DIR%
echo Archive Created: %OUTPUT_DIR%.zip
echo.
echo Next Steps:
echo 1. Test the build locally
echo 2. Deploy to your hosting environment
echo 3. Configure appsettings.json with production values
echo 4. Run database migrations
echo.
pause


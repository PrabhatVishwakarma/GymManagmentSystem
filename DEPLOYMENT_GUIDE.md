# Gym Management System - Deployment Guide

## 📦 Publishing the Application (Without Source Code)

This guide shows how to create deployable packages of your application that can run on other computers without sharing source code.

---

## 🔧 Backend (.NET API) Deployment

### Option 1: Self-Contained Deployment (Recommended)
**Includes .NET runtime - target computer doesn't need .NET installed**

#### For Windows (64-bit):
```bash
cd GymManagmentSystem
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ./publish/windows
```

#### For Windows (32-bit):
```bash
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ./publish/windows-x86
```

#### For Linux (64-bit):
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ./publish/linux
```

#### For macOS (64-bit):
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ./publish/macos
```

**Output:** A single executable file (e.g., `GymManagmentSystem.exe` for Windows)

---

### Option 2: Framework-Dependent Deployment (Smaller Size)
**Requires .NET 8.0 Runtime on target computer**

```bash
cd GymManagmentSystem
dotnet publish -c Release -o ./publish/framework-dependent
```

**Target computer requirements:**
- Must have .NET 8.0 Runtime installed
- Download from: https://dotnet.microsoft.com/download/dotnet/8.0

---

## 🎨 Frontend (React) Deployment

### Build for Production:

```bash
cd gym-management-ui
npm install
npm run build
```

**Output:** Creates a `build` folder with optimized production files

### Serve Options:

#### Option A: Using a Simple HTTP Server
```bash
# Install serve globally (one time)
npm install -g serve

# Serve the build folder
serve -s build -l 3000
```

#### Option B: Using IIS (Windows)
1. Copy the `build` folder to `C:\inetpub\wwwroot\gym-ui`
2. Configure IIS to serve static files
3. Add `web.config` for proper routing

#### Option C: Using Nginx (Linux/Windows)
1. Copy `build` folder to web server directory
2. Configure Nginx to serve static files
3. Set up reverse proxy for API calls

---

## 📁 Complete Distribution Package

### What to Include:

```
GymManagementSystem-Distribution/
│
├── Backend/
│   ├── GymManagmentSystem.exe (or .dll)
│   ├── appsettings.json
│   ├── appsettings.Production.json (optional)
│   └── (other DLL files if framework-dependent)
│
├── Frontend/
│   └── (all files from 'build' folder)
│
├── Installation-Instructions.txt
└── README.txt
```

---

## 🔐 Important: Secure Configuration

### Before Distribution, Update appsettings.json:

**DON'T include sensitive data in the package!**

Instead, create `appsettings.Production.json`:

```json
{
  "MongoDB": {
    "ConnectionString": "YOUR_PRODUCTION_CONNECTION_STRING",
    "DatabaseName": "GymManagementDB"
  },
  "JWT": {
    "Secret": "CHANGE_THIS_TO_A_SECURE_SECRET_KEY_32_CHARS_MIN"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@gmail.com"
  }
}
```

**Best Practice:** Use environment variables or external configuration:
- MongoDB connection from environment variable
- JWT secret from secure vault
- Email credentials from configuration service

---

## 🚀 Quick Deployment Scripts

### Windows PowerShell Script (`deploy.ps1`):

```powershell
# Build Backend
Write-Host "Building Backend..." -ForegroundColor Green
cd GymManagmentSystem
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../publish/Backend

# Build Frontend
Write-Host "Building Frontend..." -ForegroundColor Green
cd ../gym-management-ui
npm install
npm run build

# Copy frontend build
Write-Host "Copying Frontend..." -ForegroundColor Green
Copy-Item -Path "./build/*" -Destination "../publish/Frontend" -Recurse -Force

Write-Host "Deployment Complete! Check 'publish' folder" -ForegroundColor Cyan
```

### Linux/macOS Shell Script (`deploy.sh`):

```bash
#!/bin/bash

echo "Building Backend..."
cd GymManagmentSystem
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o ../publish/Backend

echo "Building Frontend..."
cd ../gym-management-ui
npm install
npm run build

echo "Copying Frontend..."
cp -r ./build/* ../publish/Frontend/

echo "Deployment Complete! Check 'publish' folder"
```

---

## 💻 Running on Target Computer

### Backend:

**Windows:**
```cmd
cd Backend
GymManagmentSystem.exe
```

**Linux/macOS:**
```bash
cd Backend
chmod +x GymManagmentSystem
./GymManagmentSystem
```

The API will start on: `http://localhost:5000` and `https://localhost:5001`

### Frontend:

**Using serve:**
```bash
npm install -g serve
cd Frontend
serve -s . -l 3000
```

**Or deploy to:**
- IIS (Windows)
- Nginx (Linux)
- Apache (Linux)
- Azure Static Web Apps
- Netlify
- Vercel

---

## 📊 Distribution File Sizes (Approximate)

| Deployment Type | Size |
|----------------|------|
| Self-Contained (Windows) | ~70-100 MB |
| Self-Contained (Linux) | ~70-100 MB |
| Framework-Dependent | ~5-10 MB |
| Frontend Build | ~5-15 MB |

---

## 🔒 Security Checklist Before Distribution

- [ ] Remove all hardcoded passwords
- [ ] Change JWT secret key
- [ ] Use production MongoDB connection
- [ ] Enable HTTPS in production
- [ ] Set appropriate CORS origins
- [ ] Disable debug logging
- [ ] Review exposed API endpoints
- [ ] Implement rate limiting
- [ ] Set up proper authentication
- [ ] Create installation documentation

---

## 🌐 Cloud Deployment Options

### Backend:
- **Azure App Service**
- **AWS Elastic Beanstalk**
- **Google Cloud Run**
- **Heroku**
- **DigitalOcean App Platform**

### Frontend:
- **Netlify** (Free tier available)
- **Vercel** (Free tier available)
- **Azure Static Web Apps**
- **AWS S3 + CloudFront**
- **GitHub Pages**

---

## 📝 Installation Instructions Template

Create this file for users:

```
=== Gym Management System - Installation Instructions ===

SYSTEM REQUIREMENTS:
- Windows 10/11, Linux, or macOS
- 4GB RAM minimum
- 500MB free disk space
- Internet connection for MongoDB Atlas

INSTALLATION STEPS:

1. Extract all files from the zip archive

2. Configure Backend:
   - Open Backend/appsettings.json
   - Update MongoDB connection string with your credentials
   - Update Email settings if needed

3. Run Backend:
   - Windows: Double-click GymManagmentSystem.exe
   - Linux/Mac: Run ./GymManagmentSystem in terminal
   - API will start on http://localhost:5000

4. Run Frontend:
   - Install Node.js if not already installed
   - Open command prompt in Frontend folder
   - Run: npm install -g serve
   - Run: serve -s . -l 3000
   - Open browser to http://localhost:3000

5. Default Admin Login:
   - Email: admin@gym.com
   - Password: Admin@123

TROUBLESHOOTING:
- If port 5000 is in use, the backend will use the next available port
- Check firewall settings if connection fails
- Ensure MongoDB Atlas IP whitelist includes your IP

SUPPORT:
- Contact: your-email@example.com
```

---

## 🎯 Complete Deployment Example

### Quick One-Command Deploy:

```bash
# Create complete distribution package
cd GymManagmentSystem && \
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ../Distribution/Backend && \
cd ../gym-management-ui && \
npm run build && \
xcopy /E /I build ..\Distribution\Frontend
```

This creates a ready-to-distribute `Distribution` folder!

---

## 📦 Creating an Installer (Optional)

### Windows Installer Options:
1. **Inno Setup** (Free) - Creates .exe installer
2. **WiX Toolset** (Free) - MSI installer
3. **Advanced Installer** (Commercial)

### Create ZIP Distribution:
```powershell
Compress-Archive -Path ./Distribution/* -DestinationPath GymManagementSystem-v1.0.zip
```

---

## ✅ Verification Steps

After deployment, verify:

1. Backend API responds: `http://localhost:5000/api/health` (if implemented)
2. Frontend loads: `http://localhost:3000`
3. Login works with test credentials
4. Database connection successful
5. All features functional

---

## 🔄 Updates and Versioning

When releasing updates:

1. Increment version in project files
2. Create changelog documenting changes
3. Test thoroughly in staging environment
4. Backup production database before updating
5. Deploy during low-traffic periods
6. Monitor for errors after deployment

---

**Need Help?** Check the main README or contact support.

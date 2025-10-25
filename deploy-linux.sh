#!/bin/bash

# Gym Management System - Linux/macOS Deployment Script
# This script creates a complete deployment package

echo "====================================="
echo "Gym Management System - Deployment"
echo "====================================="
echo ""

# Create distribution directory
DIST_PATH="./Distribution"
if [ -d "$DIST_PATH" ]; then
    echo "Cleaning existing distribution folder..."
    rm -rf "$DIST_PATH"
fi
mkdir -p "$DIST_PATH/Backend"
mkdir -p "$DIST_PATH/Frontend"

# Build Backend
echo ""
echo "Building Backend (Self-Contained)..."
cd GymManagmentSystem
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o "../$DIST_PATH/Backend"

if [ $? -ne 0 ]; then
    echo "Backend build failed!"
    cd ..
    exit 1
fi

cd ..
echo "✓ Backend built successfully"

# Build Frontend
echo ""
echo "Building Frontend..."
cd gym-management-ui

# Check if node_modules exists
if [ ! -d "node_modules" ]; then
    echo "Installing frontend dependencies..."
    npm install
fi

npm run build

if [ $? -ne 0 ]; then
    echo "Frontend build failed!"
    cd ..
    exit 1
fi

# Copy frontend build
echo "Copying Frontend files..."
cp -r build/* "../$DIST_PATH/Frontend/"

cd ..
echo "✓ Frontend built successfully"

# Make backend executable
chmod +x "$DIST_PATH/Backend/GymManagmentSystem"

# Create installation instructions
echo ""
echo "Creating installation instructions..."

cat > "$DIST_PATH/INSTALLATION-INSTRUCTIONS.txt" << 'EOF'
====================================================
  GYM MANAGEMENT SYSTEM - INSTALLATION GUIDE
====================================================

SYSTEM REQUIREMENTS:
--------------------
- Linux (Ubuntu 20.04+, Debian 10+, CentOS 8+) or macOS
- 4GB RAM minimum
- 500MB free disk space
- Internet connection for MongoDB Atlas

INSTALLATION STEPS:
-------------------

1. BACKEND SETUP:
   
   a) Navigate to the 'Backend' folder
   
   b) Make executable (if not already):
      chmod +x GymManagmentSystem
   
   c) IMPORTANT - Configure appsettings.json:
      - Open appsettings.json in a text editor
      - Update MongoDB connection string with your credentials
      - Update JWT secret key (keep it secure!)
      - Configure Email settings if needed
   
   d) Run the application:
      ./GymManagmentSystem
      - The API will start on: http://localhost:5000
   
   e) Keep this terminal open while using the application

2. FRONTEND SETUP:
   
   a) Install Node.js (if not installed):
      Ubuntu/Debian: sudo apt install nodejs npm
      CentOS/RHEL: sudo yum install nodejs npm
      macOS: brew install node
   
   b) Navigate to the 'Frontend' folder
   
   c) Open terminal in this folder and run:
      sudo npm install -g serve
      serve -s . -l 3000
   
   d) Open your browser and go to: http://localhost:3000

3. FIRST TIME LOGIN:
   
   Default Admin Credentials:
   - Email: admin@gym.com
   - Password: Admin@123
   
   IMPORTANT: Change the admin password after first login!

RUNNING AS SERVICE (Optional):
-------------------------------

To run backend as a systemd service on Linux:

1. Create service file:
   sudo nano /etc/systemd/system/gym-api.service

2. Add content:
   [Unit]
   Description=Gym Management System API
   After=network.target

   [Service]
   Type=simple
   User=your-username
   WorkingDirectory=/path/to/Distribution/Backend
   ExecStart=/path/to/Distribution/Backend/GymManagmentSystem
   Restart=on-failure

   [Install]
   WantedBy=multi-user.target

3. Enable and start:
   sudo systemctl enable gym-api
   sudo systemctl start gym-api
   sudo systemctl status gym-api

TROUBLESHOOTING:
----------------

Problem: "Permission denied"
Solution: Run: chmod +x GymManagmentSystem

Problem: "Cannot connect to MongoDB"
Solution: Check your internet connection and MongoDB connection string
         Verify IP whitelist in MongoDB Atlas includes your IP

Problem: Frontend shows "Network Error"
Solution: Ensure backend is running on http://localhost:5000
         Check CORS settings in appsettings.json

Problem: Port already in use
Solution: Change port in Program.cs or kill process using the port

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

SECURITY NOTES:
---------------
⚠ Do not share appsettings.json with sensitive data
⚠ Use strong passwords for admin accounts
⚠ Keep JWT secret key confidential
⚠ Regularly backup your MongoDB database
⚠ Use HTTPS in production environments
⚠ Set proper file permissions (chmod 600 appsettings.json)

SUPPORT:
--------
For issues or questions:
- Check the README.txt file
- Review MongoDB Atlas documentation
- Ensure all requirements are met

====================================================
              Thank you for using our system!
====================================================
EOF

# Create start scripts
echo "Creating start scripts..."

cat > "$DIST_PATH/start-backend.sh" << 'EOF'
#!/bin/bash
echo "Starting Gym Management System Backend..."
echo ""
echo "API will be available at: http://localhost:5000"
echo "Keep this terminal open while using the application"
echo ""
echo "Press Ctrl+C to stop the server"
echo ""
cd Backend
./GymManagmentSystem
EOF

chmod +x "$DIST_PATH/start-backend.sh"

cat > "$DIST_PATH/start-frontend.sh" << 'EOF'
#!/bin/bash
echo "Starting Gym Management System Frontend..."
echo ""
echo "Application will be available at: http://localhost:3000"
echo "Your browser should open automatically"
echo ""
echo "Keep this terminal open while using the application"
echo "Press Ctrl+C to stop the server"
echo ""
cd Frontend
serve -s . -l 3000
EOF

chmod +x "$DIST_PATH/start-frontend.sh"

# Create README
cat > "$DIST_PATH/README.txt" << EOF
# Gym Management System - Distribution Package

This package contains a complete, ready-to-deploy version of the Gym Management System.

## Quick Start

1. Read INSTALLATION-INSTRUCTIONS.txt first
2. Configure Backend/appsettings.json with your MongoDB credentials
3. Run: ./start-backend.sh to start the API server
4. Run: ./start-frontend.sh to start the web interface
5. Open browser to http://localhost:3000

## Package Contents

- Backend: .NET 8 API (Self-contained, no .NET installation required)
- Frontend: React web application (optimized production build)
- Documentation: Installation and configuration guides

## Version Information

- Build Date: $(date "+%Y-%m-%d %H:%M:%S")
- Backend: .NET 8.0
- Frontend: React 18
- Database: MongoDB Atlas

For detailed instructions, see INSTALLATION-INSTRUCTIONS.txt
EOF

# Create tar.gz archive
echo ""
echo "Creating distribution archive..."
TAR_NAME="GymManagementSystem-v1.0-$(date +%Y%m%d).tar.gz"
tar -czf "$TAR_NAME" -C "$DIST_PATH" .

echo ""
echo "====================================="
echo "✓ DEPLOYMENT COMPLETE!"
echo "====================================="
echo ""
echo "Distribution folder: $DIST_PATH"
echo "Archive: $TAR_NAME"
echo ""
echo "Package Size:"
du -sh "$DIST_PATH"
echo ""
echo "Next Steps:"
echo "1. Copy '$TAR_NAME' to target computer"
echo "2. Extract: tar -xzf $TAR_NAME"
echo "3. Follow INSTALLATION-INSTRUCTIONS.txt"
echo ""
echo "IMPORTANT: Update appsettings.json with production credentials!"
echo ""


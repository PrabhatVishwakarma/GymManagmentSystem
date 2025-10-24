# Gym Management System - Installation Guide for Customers

## Table of Contents
1. [System Requirements](#system-requirements)
2. [Installation Steps](#installation-steps)
3. [Configuration](#configuration)
4. [First-Time Setup](#first-time-setup)
5. [Accessing the System](#accessing-the-system)
6. [Troubleshooting](#troubleshooting)
7. [Support](#support)

---

## System Requirements

### Minimum Requirements
- **Operating System**: Windows Server 2016 or later / Windows 10/11
- **Processor**: Dual-core 2.0 GHz or faster
- **RAM**: 4 GB minimum (8 GB recommended)
- **Storage**: 10 GB free space
- **Database**: SQL Server 2016 or later (Express, Standard, or Enterprise)
- **Network**: Internet connection for activation and updates

### Software Prerequisites
- SQL Server 2016 or later (we can help you install SQL Server Express for free)
- .NET 8.0 Runtime (included in our installer)
- Modern web browser (Chrome, Firefox, Edge, or Safari)

---

## Installation Steps

### Step 1: Prepare Your Server

1. **Ensure SQL Server is installed**
   - If you don't have SQL Server, we recommend SQL Server Express (free)
   - Download from: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
   - Select "Express" edition
   - Choose "Basic" installation for simplest setup

2. **Create Database** (if using existing SQL Server)
   ```sql
   CREATE DATABASE GymManagementDB;
   ```

3. **Create SQL Server Login** (optional - for security)
   ```sql
   CREATE LOGIN GymMgmtUser WITH PASSWORD = 'YourSecurePassword123!';
   USE GymManagementDB;
   CREATE USER GymMgmtUser FOR LOGIN GymMgmtUser;
   ALTER ROLE db_owner ADD MEMBER GymMgmtUser;
   ```

### Step 2: Install Gym Management System

1. **Run the Installer**
   - Double-click `GymManagementSystem_Setup.exe`
   - Click "Next" on the welcome screen
   - Accept the license agreement
   - Choose installation location (default: `C:\Program Files\GymManagementSystem\`)
   - Click "Install"

2. **Wait for Installation**
   - The installer will copy all necessary files
   - Install required components
   - Register Windows Service

3. **Complete Installation**
   - Click "Finish" when installation completes
   - The Configuration Wizard will launch automatically

### Step 3: Configure Database Connection

The Configuration Wizard will guide you through setup:

1. **Database Configuration Screen**
   ```
   Server Name: localhost\SQLEXPRESS
   (or your SQL Server instance name)
   
   Database Name: GymManagementDB
   
   Authentication:
   - Windows Authentication (recommended for single server)
   - SQL Server Authentication (if you created a login in Step 1)
   
   Username: GymMgmtUser (if using SQL Authentication)
   Password: YourSecurePassword123!
   ```

2. **Test Connection**
   - Click "Test Connection" button
   - You should see "Connection Successful"
   - If not, verify your SQL Server is running and credentials are correct

3. **Initialize Database**
   - Click "Initialize Database"
   - This will create all necessary tables and initial data
   - Wait for completion (usually 30-60 seconds)

### Step 4: Configure Application Settings

1. **Admin Account Setup**
   ```
   Admin Email: admin@yourgym.com
   Password: [Choose a strong password]
   First Name: [Your name]
   Last Name: [Your name]
   ```
   
   ⚠️ **Important**: Remember these credentials - you'll need them to log in!

2. **Gym Information**
   ```
   Gym Name: Your Gym Name
   Address: Your gym address
   Phone: Your contact number
   Email: info@yourgym.com
   ```

3. **Service Configuration**
   ```
   API Port: 5202 (default)
   Web UI Port: 3000 (default)
   
   Change only if these ports are already in use.
   ```

4. **Finish Configuration**
   - Click "Save & Start Service"
   - The system will start automatically

---

## Configuration

### Manual Configuration (Advanced)

If you need to manually edit configuration:

1. **Navigate to Installation Directory**
   ```
   C:\Program Files\GymManagementSystem\
   ```

2. **Edit Configuration File**
   - File: `appsettings.json`
   - Right-click → Open with Notepad

3. **Key Settings**

   **Database Connection:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=GymManagementDB;Integrated Security=True;TrustServerCertificate=True;"
     }
   }
   ```

   **For SQL Authentication:**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=GymManagementDB;User Id=GymMgmtUser;Password=YourPassword;TrustServerCertificate=True;"
     }
   }
   ```

4. **Restart Service**
   - Open Services (Win + R, type `services.msc`)
   - Find "Gym Management System"
   - Right-click → Restart

### Firewall Configuration

If accessing from other computers on your network:

1. **Open Windows Firewall**
   - Control Panel → System and Security → Windows Defender Firewall
   - Click "Advanced settings"

2. **Create Inbound Rule**
   - Click "Inbound Rules" → "New Rule"
   - Rule Type: Port
   - Protocol: TCP
   - Specific local ports: 5202,3000
   - Action: Allow the connection
   - Profile: Select all (Domain, Private, Public)
   - Name: Gym Management System

---

## First-Time Setup

### Step 1: Access the Application

1. **Open your web browser**
2. **Navigate to:**
   ```
   http://localhost:3000
   ```
   
   Or from another computer on your network:
   ```
   http://[SERVER_IP]:3000
   ```
   
   To find your server IP:
   - Open Command Prompt
   - Type: `ipconfig`
   - Look for "IPv4 Address"

### Step 2: First Login

1. **Login Screen**
   - Email: The admin email you created during setup
   - Password: The admin password you created

2. **You're in!** 🎉

### Step 3: Initial Setup Tasks

1. **Create Membership Plans**
   - Go to "Membership Plans" menu
   - Click "Add New Plan"
   - Create your first membership plan (e.g., "Monthly Membership")

2. **Add Staff Users**
   - Go to "Users" menu
   - Click "Add User"
   - Create accounts for your staff

3. **Configure Your Gym Details**
   - Go to "Settings" (if available)
   - Update gym logo, colors, etc.

4. **Start Adding Members**
   - Go to "Enquiries" to add potential members
   - Or go to "Members" to add existing members directly

---

## Accessing the System

### From Main Server
```
http://localhost:3000
```

### From Other Computers (Same Network)
```
http://[SERVER_IP]:3000
```

Example: `http://192.168.1.100:3000`

### From Internet (Advanced Setup Required)
You'll need to:
1. Configure port forwarding on your router
2. Set up dynamic DNS (if you don't have static IP)
3. Secure your server (HTTPS, strong passwords)
4. Consider using a VPN for remote access

⚠️ **Security Warning**: Don't expose your system to the internet without proper security measures!

---

## Troubleshooting

### Problem: Cannot Access the Web Interface

**Solution 1: Check if Service is Running**
1. Press Win + R, type `services.msc`
2. Find "Gym Management System"
3. Check if Status is "Running"
4. If not, right-click → Start

**Solution 2: Check Firewall**
- Temporarily disable firewall to test
- If it works, add firewall rule (see Firewall Configuration section)

**Solution 3: Verify Ports**
- Ensure no other application is using port 5202 or 3000
- Open Command Prompt as Administrator:
  ```
  netstat -ano | findstr :5202
  netstat -ano | findstr :3000
  ```

### Problem: Database Connection Error

**Solution 1: Verify SQL Server is Running**
1. Open SQL Server Configuration Manager
2. Check "SQL Server Services"
3. Ensure your SQL Server instance is running

**Solution 2: Test Connection String**
- Open SQL Server Management Studio
- Try connecting with same credentials
- If you can't connect, fix SQL Server access first

**Solution 3: Check Connection String**
- Verify server name is correct (e.g., `localhost\SQLEXPRESS`)
- Verify database name exists
- Check username/password (if using SQL Authentication)

### Problem: Forgot Admin Password

**Solution:**
Contact support for password reset instructions.

### Problem: API Returns 500 Error

**Solution:**
1. Check logs:
   ```
   C:\Program Files\GymManagementSystem\Logs\
   ```
2. Look for error messages
3. Most common: Database connection issue or migration not run

### Problem: "This site can't be reached"

**Solution:**
1. Verify you're using correct URL: `http://localhost:3000`
2. Check if service is running (see above)
3. Try accessing from same computer first
4. Clear browser cache
5. Try different browser

---

## Running as Windows Service

The application automatically runs as a Windows Service:

### Service Management

**Start Service:**
```
net start "Gym Management System"
```

**Stop Service:**
```
net stop "Gym Management System"
```

**Restart Service:**
```
net stop "Gym Management System" && net start "Gym Management System"
```

### Auto-Start Configuration

The service is configured to start automatically when Windows starts.

To change:
1. Open Services (services.msc)
2. Find "Gym Management System"
3. Right-click → Properties
4. Startup type: Automatic, Manual, or Disabled

---

## Backup and Restore

### Automated Daily Backups (Recommended)

The system automatically backs up your database daily at 2:00 AM.

**Backup Location:**
```
C:\Program Files\GymManagementSystem\Backups\
```

### Manual Backup

**Using SQL Server Management Studio:**
1. Connect to your SQL Server
2. Right-click `GymManagementDB` → Tasks → Back Up
3. Choose destination
4. Click OK

**Using Command Line:**
```sql
BACKUP DATABASE GymManagementDB 
TO DISK = 'C:\Backups\GymManagementDB.bak'
WITH FORMAT;
```

### Restore from Backup

1. Stop the Gym Management System service
2. In SQL Server Management Studio:
   - Right-click `GymManagementDB` → Tasks → Restore → Database
   - Select backup file
   - Click OK
3. Start the service

---

## Performance Optimization

### For Best Performance

1. **Regular Database Maintenance**
   - Rebuild indexes monthly
   - Update statistics weekly

2. **Keep System Updated**
   - Install updates when notified
   - Usually includes performance improvements

3. **Monitor Disk Space**
   - Ensure at least 5 GB free space
   - Clean old log files periodically

4. **Use SSD for Database**
   - SQL Server performs much better on SSD
   - Consider upgrading if using traditional hard drive

---

## Support

### Getting Help

**Email Support:**
- Email: support@yourdomain.com
- Response time: 24-48 hours

**Phone Support:**
- Phone: +1-XXX-XXX-XXXX
- Hours: Monday-Friday, 9 AM - 5 PM EST

**Help Documentation:**
- Online: https://docs.yourdomain.com
- Video tutorials: https://youtube.com/yourdomain

**Emergency Support:**
- For critical issues: emergency@yourdomain.com
- Response time: 4 hours

### Before Contacting Support

Please gather this information:
1. What were you trying to do?
2. What actually happened?
3. Any error messages (copy full text)
4. Screenshot of the issue
5. Your system information:
   - Windows version
   - SQL Server version
   - Application version (visible in About page)

### Remote Support

We can remotely access your system to troubleshoot:
1. Install TeamViewer or AnyDesk
2. Provide us the connection code
3. We'll fix the issue while you watch

---

## Updating the System

### Automatic Updates (If Enabled)

The system will notify you when updates are available:
1. You'll see a notification in the application
2. Click "Update Now"
3. System will download and install update
4. System will restart automatically
5. Done!

### Manual Updates

1. Download update package from customer portal
2. Stop the service
3. Run the update installer
4. Service will restart automatically

⚠️ **Always backup before updating!**

---

## Security Best Practices

### Recommended Security Measures

1. **Use Strong Passwords**
   - Minimum 12 characters
   - Mix of letters, numbers, symbols
   - Change every 90 days

2. **Regular Backups**
   - Keep backups in separate location
   - Test restore process quarterly

3. **Limit Access**
   - Only give access to staff who need it
   - Remove accounts for former employees immediately

4. **Keep Software Updated**
   - Install updates promptly
   - Subscribe to security notifications

5. **Use Antivirus**
   - Keep antivirus software updated
   - Scan regularly

6. **Secure Physical Access**
   - Server should be in locked room
   - Only authorized personnel have access

---

## Frequently Asked Questions (FAQ)

**Q: Can I install this on multiple computers?**
A: The server (backend) runs on one computer. Staff can access the web interface from any computer on your network using a web browser.

**Q: Do I need internet connection?**
A: Not for daily operation. Internet is only needed for activation, updates, and if using cloud backups.

**Q: Can I customize the system?**
A: Custom development is available for Enterprise license holders. Contact us for pricing.

**Q: How many users can use this simultaneously?**
A: Depends on your license tier and server capacity. Standard license supports up to 10 concurrent users.

**Q: What if I need help after business hours?**
A: Emergency support is available for Premium and Enterprise customers. Standard customers can email and we'll respond next business day.

**Q: Can I export my data?**
A: Yes! You can export reports to Excel and PDF. Full database export is also available.

**Q: What happens if my license expires?**
A: System continues working but you won't receive updates or support. Renew promptly to maintain coverage.

---

## License Activation

### First-Time Activation

1. **Launch Application**
2. **Enter License Key**
   - You received this via email after purchase
   - Format: XXXX-XXXX-XXXX-XXXX-XXXX

3. **Activate Online** (Recommended)
   - System connects to activation server
   - Instant activation

4. **Offline Activation** (If No Internet)
   - Click "Offline Activation"
   - System generates an activation request code
   - Email this code to: activation@yourdomain.com
   - We'll send you an activation response code
   - Enter response code to complete activation

### License Information

**View Your License:**
- Help → About → License Information

**Renew License:**
- Contact sales before expiration
- Or visit customer portal: https://portal.yourdomain.com

---

## System Requirements Summary

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| OS | Windows Server 2016 / Windows 10 | Windows Server 2022 / Windows 11 |
| CPU | 2 cores @ 2.0 GHz | 4 cores @ 2.5+ GHz |
| RAM | 4 GB | 8+ GB |
| Storage | 10 GB | 50+ GB SSD |
| Database | SQL Server 2016 Express | SQL Server 2019+ Standard |
| Network | 100 Mbps | 1 Gbps |
| Browser | Chrome 90+ | Chrome Latest |

---

## Congratulations! 🎉

You're now ready to use your Gym Management System!

**Next Steps:**
1. ✅ Log in with your admin account
2. ✅ Create your first membership plan
3. ✅ Add your staff users
4. ✅ Start adding members!

**Need help?** Contact support anytime!

---

**Document Version:** 1.0  
**Last Updated:** October 2024  
**Software Version:** 1.0.0

© 2024 Gym Management System. All rights reserved.


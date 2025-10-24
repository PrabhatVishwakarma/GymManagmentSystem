# License System Implementation Guide

## Overview
This guide explains how to implement a licensing system to protect your Gym Management System and control customer access without giving them your source code.

## Why You Need Licensing

1. **Prevent Unauthorized Use**: Ensure only paying customers can use your software
2. **Control Distribution**: Stop customers from installing on unlimited machines
3. **Enable Trial Periods**: Offer time-limited demos
4. **Subscription Management**: Handle monthly/yearly renewals
5. **Feature Control**: Enable/disable features based on license tier
6. **Hardware Binding**: Prevent copying to other machines

---

## Licensing Models

### 1. **Perpetual License** (One-time payment)
- Customer buys once, uses forever
- Example: Microsoft Office (older versions)
- Good for on-premise installations

### 2. **Subscription License** (Recurring payment)
- Monthly or yearly payments
- Access revoked if subscription expires
- Good for SaaS model

### 3. **Freemium** (Free + paid upgrades)
- Basic features free
- Advanced features require payment
- Good for market penetration

### 4. **Trial License** (Time-limited evaluation)
- Full access for 14-30 days
- Converts to paid or expires
- Good for sales process

---

## Recommended Approach: Online Activation with Offline Fallback

### Benefits:
✅ Validates against your central server
✅ Prevents key sharing
✅ Hardware-bound (can't copy to other machines)
✅ Can remotely revoke licenses
✅ Works offline after initial activation
✅ Tracks usage and installations

---

## Implementation Steps

### Step 1: Choose a Licensing Library

#### Option A: Standard.Licensing (Free, Open Source)
**Pros:**
- Free and open source
- Flexible
- RSA encryption
- .NET native

**Cons:**
- Requires custom implementation
- No built-in activation server

#### Option B: KeyGen.sh (Paid Service)
**Pros:**
- Fully managed
- Built-in activation server
- Dashboard for managing licenses
- Webhooks for automation

**Cons:**
- Monthly fee (~$30+/month)
- Depends on third-party service

#### Option C: Custom Implementation (Recommended for Learning)
Build your own simple licensing system:

---

## Step 2: Create License Generation System

### Database Schema (Add to your existing database)

```sql
-- Add to your database
CREATE TABLE Licenses (
    LicenseId INT PRIMARY KEY IDENTITY(1,1),
    LicenseKey NVARCHAR(100) UNIQUE NOT NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    CustomerEmail NVARCHAR(200) NOT NULL,
    LicenseType NVARCHAR(50) NOT NULL, -- Trial, Basic, Pro, Enterprise
    IssuedDate DATETIME NOT NULL DEFAULT GETDATE(),
    ExpirationDate DATETIME NULL,
    MaxActivations INT NOT NULL DEFAULT 1,
    CurrentActivations INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE LicenseActivations (
    ActivationId INT PRIMARY KEY IDENTITY(1,1),
    LicenseId INT NOT NULL,
    MachineId NVARCHAR(200) NOT NULL,
    MachineName NVARCHAR(200),
    ActivatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    LastCheckIn DATETIME NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1,
    FOREIGN KEY (LicenseId) REFERENCES Licenses(LicenseId)
);

CREATE INDEX IX_Licenses_LicenseKey ON Licenses(LicenseKey);
CREATE INDEX IX_LicenseActivations_LicenseId ON LicenseActivations(LicenseId);
CREATE INDEX IX_LicenseActivations_MachineId ON LicenseActivations(MachineId);
```

### License Key Generator

```csharp
// Add to your backend project
public class LicenseKeyGenerator
{
    public static string GenerateLicenseKey()
    {
        // Format: XXXX-XXXX-XXXX-XXXX-XXXX
        var random = new Random();
        var parts = new string[5];
        
        for (int i = 0; i < 5; i++)
        {
            parts[i] = GenerateRandomSegment();
        }
        
        return string.Join("-", parts);
    }
    
    private static string GenerateRandomSegment()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Exclude confusing chars
        var random = new Random();
        return new string(Enumerable.Range(0, 4)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }
    
    public static string GetMachineId()
    {
        // Generate unique identifier for this machine
        // Combine multiple hardware identifiers
        var cpuId = GetCpuId();
        var diskId = GetDiskId();
        var motherboardId = GetMotherboardId();
        
        var combined = $"{cpuId}{diskId}{motherboardId}";
        
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(hash);
        }
    }
    
    private static string GetCpuId()
    {
        using (var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
        {
            foreach (var obj in searcher.Get())
            {
                return obj["ProcessorId"]?.ToString() ?? "UNKNOWN";
            }
        }
        return "UNKNOWN";
    }
    
    private static string GetDiskId()
    {
        using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive"))
        {
            foreach (var obj in searcher.Get())
            {
                return obj["SerialNumber"]?.ToString()?.Trim() ?? "UNKNOWN";
            }
        }
        return "UNKNOWN";
    }
    
    private static string GetMotherboardId()
    {
        using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
        {
            foreach (var obj in searcher.Get())
            {
                return obj["SerialNumber"]?.ToString()?.Trim() ?? "UNKNOWN";
            }
        }
        return "UNKNOWN";
    }
}
```

### License Validation Service

```csharp
public class LicenseValidationService
{
    private readonly AppDbContext _context;
    private const string LICENSE_SERVER_URL = "https://yourdomain.com/api/license";
    
    public async Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey)
    {
        try
        {
            // Get machine identifier
            var machineId = LicenseKeyGenerator.GetMachineId();
            
            // Check online first
            var onlineResult = await ValidateOnlineAsync(licenseKey, machineId);
            if (onlineResult.IsValid)
            {
                // Cache license locally
                CacheLicenseLocally(licenseKey, onlineResult);
                return onlineResult;
            }
            
            // Fallback to cached license
            return ValidateCachedLicense(licenseKey, machineId);
        }
        catch (Exception)
        {
            // Network error - check cached license
            return ValidateCachedLicense(licenseKey, machineId);
        }
    }
    
    private async Task<LicenseValidationResult> ValidateOnlineAsync(string licenseKey, string machineId)
    {
        using (var httpClient = new HttpClient())
        {
            var request = new
            {
                LicenseKey = licenseKey,
                MachineId = machineId,
                MachineName = Environment.MachineName
            };
            
            var response = await httpClient.PostAsJsonAsync($"{LICENSE_SERVER_URL}/validate", request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LicenseValidationResult>();
            }
            
            return new LicenseValidationResult { IsValid = false, Message = "License validation failed" };
        }
    }
    
    private void CacheLicenseLocally(string licenseKey, LicenseValidationResult result)
    {
        var cacheData = new
        {
            LicenseKey = licenseKey,
            ExpirationDate = result.ExpirationDate,
            LicenseType = result.LicenseType,
            CustomerName = result.CustomerName,
            LastValidated = DateTime.UtcNow
        };
        
        var json = JsonSerializer.Serialize(cacheData);
        
        // Encrypt before saving
        var encrypted = EncryptString(json);
        
        // Save to local file or registry
        File.WriteAllText("license.cache", encrypted);
    }
    
    private LicenseValidationResult ValidateCachedLicense(string licenseKey, string machineId)
    {
        try
        {
            if (!File.Exists("license.cache"))
            {
                return new LicenseValidationResult { IsValid = false, Message = "No cached license found" };
            }
            
            var encrypted = File.ReadAllText("license.cache");
            var json = DecryptString(encrypted);
            var cached = JsonSerializer.Deserialize<dynamic>(json);
            
            // Verify license key matches
            if (cached.LicenseKey != licenseKey)
            {
                return new LicenseValidationResult { IsValid = false, Message = "Invalid license" };
            }
            
            // Check if still valid
            if (cached.ExpirationDate != null && DateTime.Parse(cached.ExpirationDate.ToString()) < DateTime.UtcNow)
            {
                return new LicenseValidationResult { IsValid = false, Message = "License expired" };
            }
            
            // Check if last validated is within grace period (7 days)
            var lastValidated = DateTime.Parse(cached.LastValidated.ToString());
            if ((DateTime.UtcNow - lastValidated).TotalDays > 7)
            {
                return new LicenseValidationResult 
                { 
                    IsValid = false, 
                    Message = "License requires online validation. Please connect to the internet." 
                };
            }
            
            return new LicenseValidationResult
            {
                IsValid = true,
                LicenseType = cached.LicenseType,
                CustomerName = cached.CustomerName,
                ExpirationDate = cached.ExpirationDate != null ? DateTime.Parse(cached.ExpirationDate.ToString()) : null,
                Message = "License valid (cached)"
            };
        }
        catch
        {
            return new LicenseValidationResult { IsValid = false, Message = "Error reading cached license" };
        }
    }
    
    private string EncryptString(string plainText)
    {
        // Simple encryption - use proper encryption in production
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }
    
    private string DecryptString(string cipherText)
    {
        // Simple decryption - use proper encryption in production
        var bytes = Convert.FromBase64String(cipherText);
        return Encoding.UTF8.GetString(bytes);
    }
}

public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; }
    public string LicenseType { get; set; }
    public string CustomerName { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public int DaysRemaining => ExpirationDate.HasValue 
        ? Math.Max(0, (ExpirationDate.Value - DateTime.UtcNow).Days)
        : int.MaxValue;
}
```

### License API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class LicenseController : ControllerBase
{
    private readonly AppDbContext _context;
    
    [HttpPost("validate")]
    public async Task<ActionResult<LicenseValidationResult>> ValidateLicense([FromBody] LicenseValidationRequest request)
    {
        var license = await _context.Licenses
            .FirstOrDefaultAsync(l => l.LicenseKey == request.LicenseKey && l.IsActive);
            
        if (license == null)
        {
            return Ok(new LicenseValidationResult 
            { 
                IsValid = false, 
                Message = "Invalid license key" 
            });
        }
        
        // Check expiration
        if (license.ExpirationDate.HasValue && license.ExpirationDate < DateTime.UtcNow)
        {
            return Ok(new LicenseValidationResult 
            { 
                IsValid = false, 
                Message = "License has expired" 
            });
        }
        
        // Check activation limit
        var existingActivation = await _context.LicenseActivations
            .FirstOrDefaultAsync(a => a.LicenseId == license.LicenseId && a.MachineId == request.MachineId);
            
        if (existingActivation == null)
        {
            // New activation
            if (license.CurrentActivations >= license.MaxActivations)
            {
                return Ok(new LicenseValidationResult 
                { 
                    IsValid = false, 
                    Message = $"Maximum activations ({license.MaxActivations}) reached" 
                });
            }
            
            // Create new activation
            var activation = new LicenseActivation
            {
                LicenseId = license.LicenseId,
                MachineId = request.MachineId,
                MachineName = request.MachineName,
                IsActive = true
            };
            
            _context.LicenseActivations.Add(activation);
            license.CurrentActivations++;
            await _context.SaveChangesAsync();
        }
        else
        {
            // Update last check-in
            existingActivation.LastCheckIn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        
        return Ok(new LicenseValidationResult
        {
            IsValid = true,
            LicenseType = license.LicenseType,
            CustomerName = license.CustomerName,
            ExpirationDate = license.ExpirationDate,
            Message = "License valid"
        });
    }
    
    [HttpPost("generate")]
    [Authorize(Roles = "Admin")] // Only admins can generate licenses
    public async Task<ActionResult<License>> GenerateLicense([FromBody] GenerateLicenseRequest request)
    {
        var licenseKey = LicenseKeyGenerator.GenerateLicenseKey();
        
        var license = new License
        {
            LicenseKey = licenseKey,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            LicenseType = request.LicenseType,
            ExpirationDate = request.ExpirationDate,
            MaxActivations = request.MaxActivations,
            IsActive = true
        };
        
        _context.Licenses.Add(license);
        await _context.SaveChangesAsync();
        
        // Send license key to customer via email
        // await SendLicenseEmailAsync(license);
        
        return Ok(license);
    }
    
    [HttpPost("deactivate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateLicense(string licenseKey)
    {
        var license = await _context.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);
        if (license == null)
        {
            return NotFound();
        }
        
        license.IsActive = false;
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "License deactivated successfully" });
    }
}
```

---

## Step 3: Integrate License Check into Application

### On Application Startup

```csharp
// In Program.cs or Startup
public class Program
{
    public static async Task Main(string[] args)
    {
        // Check license before starting application
        var licenseService = new LicenseValidationService();
        var licenseKey = GetStoredLicenseKey(); // From config or user input
        
        if (string.IsNullOrEmpty(licenseKey))
        {
            Console.WriteLine("No license key found. Please activate your license.");
            // Show activation dialog
            return;
        }
        
        var validationResult = await licenseService.ValidateLicenseAsync(licenseKey);
        
        if (!validationResult.IsValid)
        {
            Console.WriteLine($"License validation failed: {validationResult.Message}");
            // Show error and exit or show activation dialog
            return;
        }
        
        Console.WriteLine($"License valid for: {validationResult.CustomerName}");
        Console.WriteLine($"License type: {validationResult.LicenseType}");
        
        if (validationResult.ExpirationDate.HasValue)
        {
            Console.WriteLine($"Days remaining: {validationResult.DaysRemaining}");
        }
        
        // Continue with normal application startup
        var builder = WebApplication.CreateBuilder(args);
        // ... rest of your startup code
    }
}
```

---

## Step 4: Create License Management Portal

Create a simple web interface for managing licenses:

### Admin Portal Features:
1. Generate new licenses
2. View all licenses
3. See active activations
4. Deactivate licenses
5. Extend expiration dates
6. View usage statistics

### Customer Portal Features:
1. View license details
2. See active installations
3. Deactivate specific installations (to move to new machine)
4. Download invoice/receipt
5. Renew subscription

---

## Pricing Based on License Tiers

```csharp
public enum LicenseType
{
    Trial,      // 30 days, full features
    Basic,      // $79/month or $799/year
    Pro,        // $149/month or $1,499/year
    Enterprise  // $299/month or $2,999/year
}

public static class LicenseFeatures
{
    public static bool HasFeature(LicenseType licenseType, string feature)
    {
        return feature switch
        {
            "BasicFeatures" => true, // All licenses
            "AdvancedReports" => licenseType >= LicenseType.Pro,
            "API Access" => licenseType == LicenseType.Enterprise,
            "CustomBranding" => licenseType == LicenseType.Enterprise,
            "MultiLocation" => licenseType >= LicenseType.Pro,
            "SMSNotifications" => licenseType >= LicenseType.Pro,
            _ => false
        };
    }
    
    public static int GetMaxMembers(LicenseType licenseType)
    {
        return licenseType switch
        {
            LicenseType.Trial => 10,
            LicenseType.Basic => 100,
            LicenseType.Pro => 500,
            LicenseType.Enterprise => int.MaxValue,
            _ => 0
        };
    }
}
```

---

## Security Best Practices

1. **Never Store License Key in Plain Text**
   - Encrypt stored license keys
   - Use Windows DPAPI or similar

2. **Obfuscate Your Code**
   - Use ConfuserEx or similar tool
   - Makes reverse engineering harder

3. **Regular Online Checks**
   - Validate license every 7 days
   - Prevents long-term offline piracy

4. **Hardware Binding**
   - Tie license to machine ID
   - Prevent copying to other machines

5. **Limit Activations**
   - Allow deactivation to move machines
   - Track and limit number of installs

6. **Tamper Detection**
   - Check for modified binaries
   - Validate critical files haven't changed

---

## Testing the License System

1. **Generate Test License**
2. **Test Valid License**
3. **Test Expired License**
4. **Test Invalid License**
5. **Test Maximum Activations**
6. **Test Offline Mode**
7. **Test License Deactivation**

---

## Next Steps

1. ✅ Add license tables to database
2. ✅ Implement license generation
3. ✅ Add validation to application startup
4. ✅ Create admin portal for license management
5. ✅ Test thoroughly
6. ✅ Document activation process for customers
7. ✅ Set up automated email for license delivery

---

This licensing system will protect your software while providing a good customer experience!


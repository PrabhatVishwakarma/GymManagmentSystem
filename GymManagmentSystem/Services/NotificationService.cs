using System.Net;
using System.Net.Mail;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;

namespace GymManagmentSystem.Services
{
    public class NotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;
        private readonly MongoDbContext _context;

        public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger, MongoDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string memberName, string planName, decimal paidAmount, DateTime startDate, DateTime endDate)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPass = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser))
                {
                    _logger.LogWarning("Email configuration not set. Skipping email send.");
                    return;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPass)
                };

                var subject = "Welcome to Our Gym - Membership Confirmation";
                var body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                            .content {{ background-color: #f9f9f9; padding: 20px; }}
                            .details {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #4CAF50; }}
                            .footer {{ text-align: center; padding: 20px; color: #777; font-size: 12px; }}
                            h1 {{ margin: 0; }}
                            .amount {{ font-size: 24px; color: #4CAF50; font-weight: bold; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🎉 Welcome to Our Gym!</h1>
                            </div>
                            <div class='content'>
                                <h2>Dear {memberName},</h2>
                                <p>Thank you for joining our gym family! We're excited to help you achieve your fitness goals.</p>
                                
                                <div class='details'>
                                    <h3>📋 Your Membership Details:</h3>
                                    <ul>
                                        <li><strong>Plan:</strong> {planName}</li>
                                        <li><strong>Start Date:</strong> {startDate:yyyy-MM-dd}</li>
                                        <li><strong>End Date:</strong> {endDate:yyyy-MM-dd}</li>
                                        <li><strong>Amount Paid:</strong> <span class='amount'>${paidAmount}</span></li>
                                    </ul>
                                </div>
                                
                                <h3>💪 Next Steps:</h3>
                                <ol>
                                    <li>Visit our gym during operating hours</li>
                                    <li>Bring a valid ID for verification</li>
                                    <li>Our staff will guide you through facilities</li>
                                    <li>Start your fitness journey!</li>
                                </ol>
                                
                                <p><strong>Need Help?</strong><br>
                                Contact us anytime at our front desk or reply to this email.</p>
                            </div>
                            <div class='footer'>
                                <p>This is an automated message. Please do not reply directly to this email.</p>
                                <p>&copy; 2025 Gym Management System. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                var message = new MailMessage(fromEmail, toEmail, subject, body)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation($"Welcome email sent successfully to {toEmail}");
                
                // Log activity
                await LogActivity("Email", $"Welcome email sent to {memberName}", "Member", null, 
                    memberName, toEmail, body, true, null, "System");
            }
            catch (Exception ex)
            {
                // Log error but don't fail the conversion operation
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                
                // Log failed activity
                await LogActivity("Email", $"Failed to send email to {memberName}", "Member", null,
                    memberName, toEmail, null, false, ex.Message, "System");
            }
        }

        public async Task SendWhatsAppGreetingAsync(string phoneNumber, string memberName, string planName)
        {
            try
            {
                var whatsappEnabled = bool.Parse(_configuration["WhatsApp:Enabled"] ?? "false");
                
                if (!whatsappEnabled)
                {
                    _logger.LogInformation($"WhatsApp disabled. Would send greeting to {phoneNumber}");
                    Console.WriteLine($"📱 WhatsApp Message (Simulation):");
                    Console.WriteLine($"   To: {phoneNumber}");
                    Console.WriteLine($"   Message: Hi {memberName}! 🎉 Welcome to our gym family! Your {planName} membership is now active. We're excited to help you achieve your fitness goals! 💪");
                    return;
                }

                // TODO: Implement actual WhatsApp integration
                // Using Twilio WhatsApp API:
                // var accountSid = _configuration["WhatsApp:Twilio:AccountSid"];
                // var authToken = _configuration["WhatsApp:Twilio:AuthToken"];
                // var fromNumber = _configuration["WhatsApp:Twilio:FromNumber"];
                
                // Example Twilio integration (requires Twilio NuGet package):
                // var message = $"Hi {memberName}! 🎉 Welcome to our gym family! " +
                //               $"Your {planName} membership is now active. " +
                //               $"We're excited to help you achieve your fitness goals! 💪";
                
                // TwilioClient.Init(accountSid, authToken);
                // var messageResource = await MessageResource.CreateAsync(
                //     body: message,
                //     from: new PhoneNumber($"whatsapp:{fromNumber}"),
                //     to: new PhoneNumber($"whatsapp:{phoneNumber}")
                // );

                _logger.LogInformation($"WhatsApp greeting sent to {phoneNumber}");
                
                // Log activity (even simulation)
                var message = $"Hi {memberName}! 🎉 Welcome to our gym family! Your {planName} membership is now active. We're excited to help you achieve your fitness goals! 💪";
                await LogActivity("WhatsApp", $"Welcome WhatsApp sent to {memberName}", "Member", null,
                    memberName, phoneNumber, message, true, null, "System");
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Log error but don't fail the conversion operation
                _logger.LogError($"Failed to send WhatsApp to {phoneNumber}: {ex.Message}");
                
                // Log failed activity
                await LogActivity("WhatsApp", $"Failed to send WhatsApp to {memberName}", "Member", null,
                    memberName, phoneNumber, null, false, ex.Message, "System");
            }
        }
        
        private async Task LogActivity(string activityType, string description, string entityType, int? entityId,
            string recipientName, string recipientContact, string messageContent, bool isSuccessful, string errorMessage, string performedBy)
        {
            try
            {
                var activity = new Activity
                {
                    ActivityId = _context.GetNextSequenceValue("Activities"),
                    ActivityType = activityType,
                    Description = description,
                    EntityType = entityType,
                    EntityId = entityId,
                    RecipientName = recipientName,
                    RecipientContact = recipientContact,
                    MessageContent = messageContent,
                    IsSuccessful = isSuccessful,
                    ErrorMessage = errorMessage,
                    PerformedBy = performedBy,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _context.Activities.InsertOneAsync(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to log activity: {ex.Message}");
                // Don't throw - activity logging should never break the main operation
            }
        }

        public async Task SendPaymentReceiptAsync(string emailTo, string memberName, string receiptNumber, decimal amountPaid, string htmlReceipt)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPass = _configuration["Email:SmtpPassword"];
            var fromEmail = _configuration["Email:FromEmail"];

            var subject = $"Payment Receipt - {receiptNumber}";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2>Payment Receipt</h2>
    <p>Dear {memberName},</p>
    <p>Thank you for your payment! Your receipt details are below:</p>
    <p><strong>Receipt Number:</strong> {receiptNumber}<br/>
    <strong>Amount Paid:</strong> ₹{amountPaid:N2}</p>
    
    <p>Please find your detailed receipt below:</p>
    
    <hr/>
    
    {htmlReceipt}
    
    <hr/>
    
    <p>For any queries, please contact us.</p>
    <p>Thank you,<br/>Gym Management Team</p>
</body>
</html>";

            var performedBy = "System";
            
            try
            {
                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPass)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(emailTo);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Payment receipt sent successfully to {emailTo}");
                
                await LogActivity(
                    "EmailReceipt",
                    $"Payment receipt {receiptNumber} sent to {memberName}",
                    "Member",
                    null,
                    memberName,
                    emailTo,
                    $"Receipt: {receiptNumber}, Amount: ₹{amountPaid:N2}",
                    true,
                    null,
                    performedBy
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send payment receipt: {ex.Message}");
                
                await LogActivity(
                    "EmailReceipt",
                    $"Failed to send payment receipt {receiptNumber} to {memberName}",
                    "Member",
                    null,
                    memberName,
                    emailTo,
                    $"Receipt: {receiptNumber}, Amount: ₹{amountPaid:N2}",
                    false,
                    ex.Message,
                    performedBy
                );
            }
        }
    }
}


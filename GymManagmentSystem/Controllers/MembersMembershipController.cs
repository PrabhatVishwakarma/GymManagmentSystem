using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;
    using GymManagmentSystem.Services;
using OfficeOpenXml;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersMembershipController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PdfReceiptService _pdfService;
        private readonly NotificationService _notificationService;

        public MembersMembershipController(AppDbContext context, PdfReceiptService pdfService, NotificationService notificationService)
        {
            _context = context;
            _pdfService = pdfService;
            _notificationService = notificationService;
        }

        // GET: api/MembersMembership
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembersMembership>>> GetMembersMemberships()
        {
            return await _context.MembersMemberships
                .Include(mm => mm.Enquiry)
                .Include(mm => mm.MembershipPlan)
                .OrderByDescending(mm => mm.CreatedAt)
                .ToListAsync();
        }

        // GET: api/MembersMembership/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MembersMembership>> GetMembersMembership(int id)
        {
            var membersMembership = await _context.MembersMemberships
                .Include(mm => mm.Enquiry)
                .Include(mm => mm.MembershipPlan)
                .FirstOrDefaultAsync(mm => mm.MembersMembershipId == id);

            if (membersMembership == null)
            {
                return NotFound();
            }

            return membersMembership;
        }

        // GET: api/MembersMembership/Active
        [HttpGet("Active")]
        public async Task<ActionResult<IEnumerable<MembersMembership>>> GetActiveMemberships()
        {
            return await _context.MembersMemberships
                .Include(mm => mm.Enquiry)
                .Include(mm => mm.MembershipPlan)
                .Where(mm => mm.IsActive)
                .OrderByDescending(mm => mm.StartDate)
                .ToListAsync();
        }

        // GET: api/MembersMembership/Expired
        [HttpGet("Expired")]
        public async Task<ActionResult<IEnumerable<MembersMembership>>> GetExpiredMemberships()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.MembersMemberships
                .Include(mm => mm.Enquiry)
                .Include(mm => mm.MembershipPlan)
                .Where(mm => mm.EndDate < currentDate)
                .OrderByDescending(mm => mm.EndDate)
                .ToListAsync();
        }

        // GET: api/MembersMembership/ExpiringSoon
        [HttpGet("ExpiringSoon")]
        public async Task<ActionResult<IEnumerable<MembersMembership>>> GetExpiringSoonMemberships()
        {
            var currentDate = DateTime.UtcNow;
            var thirtyDaysFromNow = currentDate.AddDays(30);
            
            return await _context.MembersMemberships
                .Include(mm => mm.Enquiry)
                .Include(mm => mm.MembershipPlan)
                .Where(mm => mm.EndDate >= currentDate && mm.EndDate <= thirtyDaysFromNow)
                .OrderBy(mm => mm.EndDate)
                .ToListAsync();
        }

        // GET: api/MembersMembership/PendingPayments
        [HttpGet("PendingPayments")]
        public async Task<ActionResult<IEnumerable<MembersMembership>>> GetPendingPayments()
        {
            var currentDate = DateTime.UtcNow;
            return await _context.MembersMemberships
                .Include(mm => mm.Enquiry)
                .Include(mm => mm.MembershipPlan)
                .Where(mm => mm.RemainingAmount > 0 && mm.NextPaymentDueDate <= currentDate)
                .OrderBy(mm => mm.NextPaymentDueDate)
                .ToListAsync();
        }

        // POST: api/MembersMembership
        [HttpPost]
        public async Task<ActionResult<MembersMembership>> PostMembersMembership(MembersMembership membersMembership)
        {
            // Validate that enquiry and membership plan exist
            var enquiry = await _context.Enquiries.FindAsync(membersMembership.EnquiryId);
            if (enquiry == null)
            {
                return BadRequest("Enquiry not found");
            }

            var membershipPlan = await _context.MembershipPlans.FindAsync(membersMembership.MembershipPlanId);
            if (membershipPlan == null)
            {
                return BadRequest("Membership plan not found");
            }

            membersMembership.CreatedAt = DateTime.UtcNow;
            membersMembership.UpdatedAt = DateTime.UtcNow;
            
            _context.MembersMemberships.Add(membersMembership);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMembersMembership", new { id = membersMembership.MembersMembershipId }, membersMembership);
        }

        // PUT: api/MembersMembership/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMembersMembership(int id, MembersMembership membersMembership)
        {
            if (id != membersMembership.MembersMembershipId)
            {
                return BadRequest();
            }

            membersMembership.UpdatedAt = DateTime.UtcNow;
            _context.Entry(membersMembership).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MembersMembershipExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/MembersMembership/5/Payment
        [HttpPost("{id}/Payment")]
        public async Task<ActionResult<PaymentResponse>> ProcessPayment(int id, [FromBody] PaymentRequest paymentRequest)
        {
            var membersMembership = await _context.MembersMemberships
                .Include(m => m.Enquiry)
                .Include(m => m.MembershipPlan)
                .FirstOrDefaultAsync(m => m.MembersMembershipId == id);
                
            if (membersMembership == null)
            {
                return NotFound("Membership not found");
            }

            if (paymentRequest.Amount <= 0)
            {
                return BadRequest("Payment amount must be greater than 0");
            }

            if (paymentRequest.Amount > membersMembership.RemainingAmount)
            {
                return BadRequest("Payment amount cannot exceed remaining amount");
            }

            var previousPaid = membersMembership.PaidAmount;
            membersMembership.PaidAmount += paymentRequest.Amount;
            membersMembership.UpdatedAt = DateTime.UtcNow;
            membersMembership.UpdatedBy = User?.Identity?.Name ?? "System";

            // Update next payment due date if there's still remaining amount
            if (membersMembership.RemainingAmount > 0)
            {
                membersMembership.NextPaymentDueDate = DateTime.UtcNow.AddMonths(1);
            }
            else
            {
                membersMembership.NextPaymentDueDate = null;
            }

            await _context.SaveChangesAsync();

            // Create payment receipt
            var receiptCount = await _context.PaymentReceipts.CountAsync();
            var receiptNumber = $"REC-{DateTime.UtcNow:yyyy}-{(receiptCount + 1):D5}";

            var receipt = new PaymentReceipt
            {
                ReceiptNumber = receiptNumber,
                MembersMembershipId = id,
                AmountPaid = paymentRequest.Amount,
                PaymentMethod = paymentRequest.PaymentMethod ?? "Cash",
                TransactionId = paymentRequest.TransactionId,
                TotalAmount = membersMembership.TotalAmount,
                PreviousPaid = previousPaid,
                RemainingAmount = membersMembership.RemainingAmount,
                Notes = paymentRequest.Notes,
                PaymentDate = DateTime.UtcNow,
                ReceivedBy = User?.Identity?.Name ?? "Admin",
                MemberName = $"{membersMembership.Enquiry?.FirstName} {membersMembership.Enquiry?.LastName}",
                MemberEmail = membersMembership.Enquiry?.Email,
                MemberPhone = membersMembership.Enquiry?.Phone,
                PlanName = membersMembership.MembershipPlan?.PlanName,
                CreatedAt = DateTime.UtcNow,
                EmailSent = false
            };
            _context.PaymentReceipts.Add(receipt);
            await _context.SaveChangesAsync();
            
            // Generate and save HTML content to database
            receipt.HtmlContent = _pdfService.GenerateReceiptHtmlContent(receipt);
            _context.PaymentReceipts.Update(receipt);

            // Log payment activity
            var memberName = receipt.MemberName;
            var activity = new Activity
            {
                ActivityType = "PaymentReceived",
                Description = $"Payment of ${paymentRequest.Amount:N2} received from {memberName}",
                EntityType = "Member",
                EntityId = id,
                RecipientName = memberName,
                RecipientContact = membersMembership.Enquiry?.Email,
                MessageContent = $"Receipt: {receiptNumber}, Payment: ${paymentRequest.Amount:N2}, Total Paid: ${membersMembership.PaidAmount:N2}, Remaining: ${membersMembership.RemainingAmount:N2}, Method: {paymentRequest.PaymentMethod ?? "Cash"}",
                IsSuccessful = true,
                PerformedBy = User?.Identity?.Name ?? "System",
                CreatedAt = DateTime.UtcNow
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Send receipt email (async, don't wait - don't block response)
            _ = Task.Run(async () =>
            {
                try
                {
                    var htmlContent = _pdfService.GenerateReceiptHtmlContent(receipt);
                    await _notificationService.SendPaymentReceiptAsync(
                        receipt.MemberEmail,
                        receipt.MemberName,
                        receipt.ReceiptNumber,
                        receipt.AmountPaid,
                        htmlContent
                    );

                    // Update receipt to mark email as sent
                    receipt.EmailSent = true;
                    receipt.EmailSentAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send receipt email: {ex.Message}");
                }
            });

            return Ok(new
            {
                message = "Payment processed successfully",
                membersMembershipId = membersMembership.MembersMembershipId,
                paymentAmount = paymentRequest.Amount,
                remainingAmount = membersMembership.RemainingAmount,
                isFullyPaid = membersMembership.RemainingAmount <= 0,
                nextPaymentDueDate = membersMembership.NextPaymentDueDate,
                receiptNumber = receiptNumber,
                receiptId = receipt.PaymentReceiptId
            });
        }

        // PUT: api/MembersMembership/5/Renew
        [HttpPut("{id}/Renew")]
        public async Task<ActionResult<MembersMembership>> RenewMembership(int id, [FromBody] RenewalRequest renewalRequest)
        {
            var membersMembership = await _context.MembersMemberships
                .Include(mm => mm.MembershipPlan)
                .FirstOrDefaultAsync(mm => mm.MembersMembershipId == id);

            if (membersMembership == null)
            {
                return NotFound("Membership not found");
            }

            // Extend the membership
            membersMembership.StartDate = DateTime.UtcNow;
            membersMembership.PaidAmount = renewalRequest.PaidAmount;
            membersMembership.TotalAmount = membersMembership.MembershipPlan.Price;
            membersMembership.NextPaymentDueDate = DateTime.UtcNow.AddMonths(1);
            membersMembership.UpdatedAt = DateTime.UtcNow;
            membersMembership.UpdatedBy = renewalRequest.UpdatedBy;

            await _context.SaveChangesAsync();

            return Ok(membersMembership);
        }

        // PUT: api/MembersMembership/5/Upgrade
        [HttpPut("{id}/Upgrade")]
        public async Task<ActionResult<MembersMembership>> UpgradeMembership(int id, [FromBody] UpgradeRequest request)
        {
            var membership = await _context.MembersMemberships
                .Include(m => m.MembershipPlan)
                .Include(m => m.Enquiry)
                .FirstOrDefaultAsync(m => m.MembersMembershipId == id);

            if (membership == null)
            {
                return NotFound(new { message = "Membership not found" });
            }

            var newPlan = await _context.MembershipPlans.FindAsync(request.NewMembershipPlanId);
            if (newPlan == null)
            {
                return NotFound(new { message = "New membership plan not found" });
            }

            var oldPlanName = membership.MembershipPlan?.PlanName;
            var memberName = $"{membership.Enquiry?.FirstName} {membership.Enquiry?.LastName}";

            // Upgrade membership
            membership.MembershipPlanId = newPlan.MembershipPlanId;
            membership.StartDate = DateTime.UtcNow; // Reset start date for new plan
            membership.TotalAmount = newPlan.Price;
            membership.PaidAmount = request.PaidAmount;
            membership.NextPaymentDueDate = membership.RemainingAmount > 0 ? DateTime.UtcNow.AddMonths(1) : null;
            membership.UpdatedAt = DateTime.UtcNow;
            membership.UpdatedBy = request.UpdatedBy;

            await _context.SaveChangesAsync();

            // Log activity
            var activity = new Activity
            {
                ActivityType = "MembershipUpgraded",
                Description = $"Membership upgraded from {oldPlanName} to {newPlan.PlanName}",
                EntityType = "Member",
                EntityId = id,
                RecipientName = memberName,
                RecipientContact = membership.Enquiry?.Email,
                MessageContent = $"Plan: {newPlan.PlanName}, Amount: ${newPlan.Price}, Paid: ${request.PaidAmount}",
                IsSuccessful = true,
                PerformedBy = request.UpdatedBy,
                CreatedAt = DateTime.UtcNow
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Reload with new plan details
            await _context.Entry(membership).Reference(m => m.MembershipPlan).LoadAsync();

            return Ok(new 
            { 
                message = "Membership upgraded successfully",
                membership = membership 
            });
        }

        // DELETE: api/MembersMembership/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMembersMembership(int id)
        {
            var membersMembership = await _context.MembersMemberships
                .Include(m => m.Enquiry)
                .Include(m => m.MembershipPlan)
                .FirstOrDefaultAsync(m => m.MembersMembershipId == id);
                
            if (membersMembership == null)
            {
                return NotFound();
            }

            // Get member details before deleting
            var memberName = $"{membersMembership.Enquiry?.FirstName} {membersMembership.Enquiry?.LastName}";
            var planName = membersMembership.MembershipPlan?.PlanName ?? "Unknown Plan";
            var email = membersMembership.Enquiry?.Email;

            // Delete the membership
            _context.MembersMemberships.Remove(membersMembership);
            await _context.SaveChangesAsync();

            // Log the deletion activity
            var activity = new Activity
            {
                ActivityType = "MembershipDeleted",
                Description = $"Membership deleted for {memberName} - Plan: {planName}",
                EntityType = "Member",
                EntityId = id,
                RecipientName = memberName,
                RecipientContact = email,
                MessageContent = $"Deleted membership: {planName}, Total: ${membersMembership.TotalAmount}, Paid: ${membersMembership.PaidAmount}",
                IsSuccessful = true,
                PerformedBy = User?.Identity?.Name ?? "System",
                CreatedAt = DateTime.UtcNow
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/MembersMembership/Stats
        [HttpGet("Stats")]
        public async Task<ActionResult<MembershipStats>> GetMembershipStats()
        {
            var currentDate = DateTime.UtcNow;
            var thirtyDaysFromNow = currentDate.AddDays(30);

            var stats = new MembershipStats
            {
                TotalMemberships = await _context.MembersMemberships.CountAsync(),
                ActiveMemberships = await _context.MembersMemberships.CountAsync(mm => mm.IsActive),
                ExpiredMemberships = await _context.MembersMemberships.CountAsync(mm => mm.EndDate < currentDate),
                ExpiringSoon = await _context.MembersMemberships.CountAsync(mm => mm.EndDate >= currentDate && mm.EndDate <= thirtyDaysFromNow),
                PendingPayments = await _context.MembersMemberships.CountAsync(mm => mm.RemainingAmount > 0),
                TotalRevenue = await _context.MembersMemberships.SumAsync(mm => mm.PaidAmount),
                OutstandingAmount = await _context.MembersMemberships.SumAsync(mm => mm.RemainingAmount)
            };

            return stats;
        }

        // GET: api/MembersMembership/ExportToExcel
        [HttpGet("ExportToExcel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var memberships = await _context.MembersMemberships
                .Include(mm => mm.Enquiry)
                .Include(mm => mm.MembershipPlan)
                .OrderByDescending(mm => mm.CreatedAt)
                .ToListAsync();

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Members");

                // Add headers
                worksheet.Cells[1, 1].Value = "Member ID";
                worksheet.Cells[1, 2].Value = "Member Name";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Phone";
                worksheet.Cells[1, 5].Value = "Membership Plan";
                worksheet.Cells[1, 6].Value = "Duration (Months)";
                worksheet.Cells[1, 7].Value = "Start Date";
                worksheet.Cells[1, 8].Value = "End Date";
                worksheet.Cells[1, 9].Value = "Total Amount";
                worksheet.Cells[1, 10].Value = "Paid Amount";
                worksheet.Cells[1, 11].Value = "Remaining Amount";
                worksheet.Cells[1, 12].Value = "Payment Status";
                worksheet.Cells[1, 13].Value = "Membership Status";
                worksheet.Cells[1, 14].Value = "Next Payment Due";
                worksheet.Cells[1, 15].Value = "Created By";
                worksheet.Cells[1, 16].Value = "Created At";

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 16])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                }

                // Add data
                int row = 2;
                foreach (var membership in memberships)
                {
                    var currentDate = DateTime.UtcNow;
                    var isExpired = membership.EndDate < currentDate;
                    var membershipStatus = isExpired ? "Expired" : 
                                         membership.EndDate <= currentDate.AddDays(30) ? "Expiring Soon" : 
                                         "Active";
                    var paymentStatus = membership.RemainingAmount <= 0 ? "Fully Paid" : 
                                       membership.RemainingAmount < membership.TotalAmount ? "Partial" : 
                                       "Unpaid";

                    worksheet.Cells[row, 1].Value = membership.MembersMembershipId;
                    worksheet.Cells[row, 2].Value = $"{membership.Enquiry?.FirstName} {membership.Enquiry?.LastName}";
                    worksheet.Cells[row, 3].Value = membership.Enquiry?.Email;
                    worksheet.Cells[row, 4].Value = membership.Enquiry?.Phone;
                    worksheet.Cells[row, 5].Value = membership.MembershipPlan?.PlanName;
                    worksheet.Cells[row, 6].Value = membership.MembershipPlan?.DurationInMonths;
                    worksheet.Cells[row, 7].Value = membership.StartDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 8].Value = membership.EndDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 9].Value = membership.TotalAmount;
                    worksheet.Cells[row, 10].Value = membership.PaidAmount;
                    worksheet.Cells[row, 11].Value = membership.RemainingAmount;
                    worksheet.Cells[row, 12].Value = paymentStatus;
                    worksheet.Cells[row, 13].Value = membershipStatus;
                    worksheet.Cells[row, 14].Value = membership.NextPaymentDueDate?.ToString("yyyy-MM-dd") ?? "N/A";
                    worksheet.Cells[row, 15].Value = membership.CreatedBy;
                    worksheet.Cells[row, 16].Value = membership.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    row++;
                }

                // Format currency columns
                worksheet.Cells[2, 9, row - 1, 11].Style.Numberformat.Format = "$#,##0.00";

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Generate file
                var fileBytes = package.GetAsByteArray();
                var fileName = $"Members_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // PUT: api/MembersMembership/5/ToggleStatus
        [HttpPut("{id}/ToggleStatus")]
        public async Task<ActionResult<MembersMembership>> ToggleMembershipStatus(int id, [FromBody] ToggleStatusRequest request)
        {
            var membership = await _context.MembersMemberships
                .Include(m => m.Enquiry)
                .Include(m => m.MembershipPlan)
                .FirstOrDefaultAsync(m => m.MembersMembershipId == id);

            if (membership == null) return NotFound();

            membership.IsInactive = request.IsInactive;
            membership.UpdatedAt = DateTime.UtcNow;
            membership.UpdatedBy = request.UpdatedBy;

            await _context.SaveChangesAsync();

            // Log activity
            var memberName = $"{membership.Enquiry?.FirstName} {membership.Enquiry?.LastName}";
            var activity = new Activity
            {
                ActivityType = request.IsInactive ? "MembershipDeactivated" : "MembershipActivated",
                Description = $"Membership {(request.IsInactive ? "deactivated" : "activated")} for {memberName}",
                EntityType = "Member",
                EntityId = id,
                RecipientName = memberName,
                RecipientContact = membership.Enquiry?.Email,
                MessageContent = $"Status changed to: {(request.IsInactive ? "Inactive" : "Active")}",
                IsSuccessful = true,
                PerformedBy = request.UpdatedBy,
                CreatedAt = DateTime.UtcNow
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = $"Membership {(request.IsInactive ? "deactivated" : "activated")} successfully", 
                membership = membership 
            });
        }

        private bool MembersMembershipExists(int id)
        {
            return _context.MembersMemberships.Any(e => e.MembersMembershipId == id);
        }
    }

    public class RenewalRequest
    {
        public decimal PaidAmount { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class UpgradeRequest
    {
        public int NewMembershipPlanId { get; set; }
        public decimal PaidAmount { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class ToggleStatusRequest
    {
        public bool IsInactive { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public string Notes { get; set; }
    }

    public class PaymentResponse
    {
        public string Message { get; set; }
        public decimal RemainingAmount { get; set; }
        public PaymentReceipt Receipt { get; set; }
    }

    public class MembershipStats
    {
        public int TotalMemberships { get; set; }
        public int ActiveMemberships { get; set; }
        public int ExpiredMemberships { get; set; }
        public int ExpiringSoon { get; set; }
        public int PendingPayments { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal OutstandingAmount { get; set; }
    }
}

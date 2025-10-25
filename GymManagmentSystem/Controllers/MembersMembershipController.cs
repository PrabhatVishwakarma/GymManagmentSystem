using Microsoft.AspNetCore.Mvc;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;
using GymManagmentSystem.Services;
using OfficeOpenXml;
using MongoDB.Driver;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersMembershipController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly PdfReceiptService _pdfService;
        private readonly NotificationService _notificationService;

        public MembersMembershipController(MongoDbContext context, PdfReceiptService pdfService, NotificationService notificationService)
        {
            _context = context;
            _pdfService = pdfService;
            _notificationService = notificationService;
        }

        // GET: api/MembersMembership
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetMembersMemberships()
        {
            var sort = Builders<MembersMembership>.Sort.Descending(mm => mm.CreatedAt);
            var memberships = await _context.MembersMemberships.Find(_ => true).Sort(sort).ToListAsync();

            // Load related data for each membership
            var result = new List<object>();
            foreach (var membership in memberships)
            {
                var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
                var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

                var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membership.MembershipPlanId);
                var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

                result.Add(new
                {
                    membersMembershipId = membership.MembersMembershipId,
                    enquiryId = membership.EnquiryId,
                    membershipPlanId = membership.MembershipPlanId,
                    startDate = membership.StartDate,
                    endDate = membership.EndDate,
                    totalAmount = membership.TotalAmount,
                    paidAmount = membership.PaidAmount,
                    remainingAmount = membership.RemainingAmount,
                    nextPaymentDueDate = membership.NextPaymentDueDate,
                    isInactive = membership.IsInactive,
                    isActive = membership.IsActive,
                    createdBy = membership.CreatedBy,
                    createdAt = membership.CreatedAt,
                    enquiry = enquiry,
                    membershipPlan = plan
                });
            }

            return Ok(result);
        }

        // GET: api/MembersMembership/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetMembersMembership(int id)
        {
            var filter = Builders<MembersMembership>.Filter.Eq(mm => mm.MembersMembershipId, id);
            var membership = await _context.MembersMemberships.Find(filter).FirstOrDefaultAsync();

            if (membership == null)
            {
                return NotFound();
            }

            // Load related data
            var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
            var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

            var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membership.MembershipPlanId);
            var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

            return Ok(new
            {
                membersMembershipId = membership.MembersMembershipId,
                enquiryId = membership.EnquiryId,
                membershipPlanId = membership.MembershipPlanId,
                startDate = membership.StartDate,
                endDate = membership.EndDate,
                totalAmount = membership.TotalAmount,
                paidAmount = membership.PaidAmount,
                remainingAmount = membership.RemainingAmount,
                nextPaymentDueDate = membership.NextPaymentDueDate,
                isInactive = membership.IsInactive,
                isActive = membership.IsActive,
                createdBy = membership.CreatedBy,
                createdAt = membership.CreatedAt,
                enquiry = enquiry,
                membershipPlan = plan
            });
        }

        // GET: api/MembersMembership/Active
        [HttpGet("Active")]
        public async Task<ActionResult<IEnumerable<object>>> GetActiveMemberships()
        {
            var memberships = await _context.MembersMemberships.Find(_ => true).ToListAsync();
            
            // Filter active memberships in memory (since IsActive is computed)
            var activeMemberships = memberships.Where(mm => mm.IsActive).OrderByDescending(mm => mm.StartDate).ToList();

            // Load related data
            var result = new List<object>();
            foreach (var membership in activeMemberships)
            {
                var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
                var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

                var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membership.MembershipPlanId);
                var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

                result.Add(new
                {
                    membersMembershipId = membership.MembersMembershipId,
                    enquiryId = membership.EnquiryId,
                    membershipPlanId = membership.MembershipPlanId,
                    startDate = membership.StartDate,
                    endDate = membership.EndDate,
                    totalAmount = membership.TotalAmount,
                    paidAmount = membership.PaidAmount,
                    remainingAmount = membership.RemainingAmount,
                    isActive = membership.IsActive,
                    enquiry = enquiry,
                    membershipPlan = plan
                });
            }

            return Ok(result);
        }

        // GET: api/MembersMembership/Expired
        [HttpGet("Expired")]
        public async Task<ActionResult<IEnumerable<object>>> GetExpiredMemberships()
        {
            var currentDate = DateTime.UtcNow;
            var memberships = await _context.MembersMemberships.Find(_ => true).ToListAsync();
            
            // Filter expired memberships (EndDate < currentDate)
            var expiredMemberships = memberships.Where(mm => mm.EndDate < currentDate).OrderByDescending(mm => mm.EndDate).ToList();

            var result = new List<object>();
            foreach (var membership in expiredMemberships)
            {
                var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
                var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

                var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membership.MembershipPlanId);
                var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

                result.Add(new
                {
                    membersMembershipId = membership.MembersMembershipId,
                    enquiryId = membership.EnquiryId,
                    membershipPlanId = membership.MembershipPlanId,
                    startDate = membership.StartDate,
                    endDate = membership.EndDate,
                    totalAmount = membership.TotalAmount,
                    paidAmount = membership.PaidAmount,
                    enquiry = enquiry,
                    membershipPlan = plan
                });
            }

            return Ok(result);
        }

        // GET: api/MembersMembership/ExpiringSoon
        [HttpGet("ExpiringSoon")]
        public async Task<ActionResult<IEnumerable<object>>> GetExpiringSoonMemberships()
        {
            var currentDate = DateTime.UtcNow;
            var thirtyDaysFromNow = currentDate.AddDays(30);
            
            var memberships = await _context.MembersMemberships.Find(_ => true).ToListAsync();
            var expiringSoon = memberships
                .Where(mm => mm.EndDate >= currentDate && mm.EndDate <= thirtyDaysFromNow)
                .OrderBy(mm => mm.EndDate)
                .ToList();

            var result = new List<object>();
            foreach (var membership in expiringSoon)
            {
                var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
                var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

                var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membership.MembershipPlanId);
                var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

                result.Add(new
                {
                    membersMembershipId = membership.MembersMembershipId,
                    endDate = membership.EndDate,
                    enquiry = enquiry,
                    membershipPlan = plan
                });
            }

            return Ok(result);
        }

        // GET: api/MembersMembership/PendingPayments
        [HttpGet("PendingPayments")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendingPayments()
        {
            var currentDate = DateTime.UtcNow;
            var memberships = await _context.MembersMemberships.Find(_ => true).ToListAsync();
            
            var pendingPayments = memberships
                .Where(mm => mm.RemainingAmount > 0 && mm.NextPaymentDueDate <= currentDate)
                .OrderBy(mm => mm.NextPaymentDueDate)
                .ToList();

            var result = new List<object>();
            foreach (var membership in pendingPayments)
            {
                var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
                var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

                var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membership.MembershipPlanId);
                var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

                result.Add(new
                {
                    membersMembershipId = membership.MembersMembershipId,
                    remainingAmount = membership.RemainingAmount,
                    nextPaymentDueDate = membership.NextPaymentDueDate,
                    enquiry = enquiry,
                    membershipPlan = plan
                });
            }

            return Ok(result);
        }

        // POST: api/MembersMembership
        [HttpPost]
        public async Task<ActionResult<MembersMembership>> PostMembersMembership(MembersMembership membersMembership)
        {
            // Validate that enquiry and membership plan exist
            var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membersMembership.EnquiryId);
            var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();
            if (enquiry == null)
            {
                return BadRequest("Enquiry not found");
            }

            var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membersMembership.MembershipPlanId);
            var membershipPlan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();
            if (membershipPlan == null)
            {
                return BadRequest("Membership plan not found");
            }

            membersMembership.MembersMembershipId = _context.GetNextSequenceValue("MembersMemberships");
            membersMembership.DurationInMonths = membershipPlan.DurationInMonths;
            membersMembership.CreatedAt = DateTime.UtcNow;
            membersMembership.UpdatedAt = DateTime.UtcNow;
            
            await _context.MembersMemberships.InsertOneAsync(membersMembership);

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
            
            var filter = Builders<MembersMembership>.Filter.Eq(mm => mm.MembersMembershipId, id);
            var result = await _context.MembersMemberships.ReplaceOneAsync(filter, membersMembership);

            if (result.MatchedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/MembersMembership/5/Payment
        [HttpPost("{id}/Payment")]
        public async Task<ActionResult<object>> ProcessPayment(int id, [FromBody] PaymentRequest paymentRequest)
        {
            var filter = Builders<MembersMembership>.Filter.Eq(m => m.MembersMembershipId, id);
            var membersMembership = await _context.MembersMemberships.Find(filter).FirstOrDefaultAsync();
                
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

            // Load related data
            var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membersMembership.EnquiryId);
            var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

            var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membersMembership.MembershipPlanId);
            var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

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

            await _context.MembersMemberships.ReplaceOneAsync(filter, membersMembership);

            // Create payment receipt
            var receiptCount = await _context.PaymentReceipts.CountDocumentsAsync(_ => true);
            var receiptNumber = $"REC-{DateTime.UtcNow:yyyy}-{(receiptCount + 1):D5}";

            var receipt = new PaymentReceipt
            {
                PaymentReceiptId = _context.GetNextSequenceValue("PaymentReceipts"),
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
                MemberName = $"{enquiry?.FirstName} {enquiry?.LastName}",
                MemberEmail = enquiry?.Email,
                MemberPhone = enquiry?.Phone,
                PlanName = plan?.PlanName,
                CreatedAt = DateTime.UtcNow,
                EmailSent = false
            };
            await _context.PaymentReceipts.InsertOneAsync(receipt);
            
            // Generate and save HTML content to database
            receipt.HtmlContent = _pdfService.GenerateReceiptHtmlContent(receipt);
            var receiptFilter = Builders<PaymentReceipt>.Filter.Eq(r => r.PaymentReceiptId, receipt.PaymentReceiptId);
            await _context.PaymentReceipts.ReplaceOneAsync(receiptFilter, receipt);

            // Log payment activity
            var memberName = receipt.MemberName;
            var activity = new Activity
            {
                ActivityId = _context.GetNextSequenceValue("Activities"),
                ActivityType = "PaymentReceived",
                Description = $"Payment of ${paymentRequest.Amount:N2} received from {memberName}",
                EntityType = "Member",
                EntityId = id,
                RecipientName = memberName,
                RecipientContact = enquiry?.Email,
                MessageContent = $"Receipt: {receiptNumber}, Payment: ${paymentRequest.Amount:N2}, Total Paid: ${membersMembership.PaidAmount:N2}, Remaining: ${membersMembership.RemainingAmount:N2}, Method: {paymentRequest.PaymentMethod ?? "Cash"}",
                IsSuccessful = true,
                PerformedBy = User?.Identity?.Name ?? "System",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Activities.InsertOneAsync(activity);

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
                    var updateFilter = Builders<PaymentReceipt>.Filter.Eq(r => r.PaymentReceiptId, receipt.PaymentReceiptId);
                    await _context.PaymentReceipts.ReplaceOneAsync(updateFilter, receipt);
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
        [HttpPost("{id}/Renew")]
        public async Task<ActionResult<MembersMembership>> RenewMembership(int id, [FromBody] RenewalRequest renewalRequest)
        {
            var filter = Builders<MembersMembership>.Filter.Eq(mm => mm.MembersMembershipId, id);
            var membersMembership = await _context.MembersMemberships.Find(filter).FirstOrDefaultAsync();

            if (membersMembership == null)
            {
                return NotFound("Membership not found");
            }

            var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membersMembership.MembershipPlanId);
            var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

            // Extend the membership
            membersMembership.StartDate = DateTime.UtcNow;
            membersMembership.PaidAmount = renewalRequest.PaidAmount;
            membersMembership.TotalAmount = plan.Price;
            membersMembership.DurationInMonths = plan.DurationInMonths;
            membersMembership.NextPaymentDueDate = DateTime.UtcNow.AddMonths(1);
            membersMembership.UpdatedAt = DateTime.UtcNow;
            membersMembership.UpdatedBy = renewalRequest.UpdatedBy;

            await _context.MembersMemberships.ReplaceOneAsync(filter, membersMembership);

            return Ok(membersMembership);
        }

        // PUT: api/MembersMembership/5/Upgrade
        [HttpPut("{id}/Upgrade")]
        public async Task<ActionResult<object>> UpgradeMembership(int id, [FromBody] UpgradeRequest request)
        {
            var filter = Builders<MembersMembership>.Filter.Eq(m => m.MembersMembershipId, id);
            var membership = await _context.MembersMemberships.Find(filter).FirstOrDefaultAsync();

            if (membership == null)
            {
                return NotFound(new { message = "Membership not found" });
            }

            var newPlanFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, request.NewMembershipPlanId);
            var newPlan = await _context.MembershipPlans.Find(newPlanFilter).FirstOrDefaultAsync();
            if (newPlan == null)
            {
                return NotFound(new { message = "New membership plan not found" });
            }

            var oldPlanFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membership.MembershipPlanId);
            var oldPlan = await _context.MembershipPlans.Find(oldPlanFilter).FirstOrDefaultAsync();
            var oldPlanName = oldPlan?.PlanName;

            var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
            var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();
            var memberName = $"{enquiry?.FirstName} {enquiry?.LastName}";

            // Upgrade membership
            membership.MembershipPlanId = newPlan.MembershipPlanId;
            membership.StartDate = DateTime.UtcNow;
            membership.DurationInMonths = newPlan.DurationInMonths;
            membership.TotalAmount = newPlan.Price;
            membership.PaidAmount = request.PaidAmount;
            membership.NextPaymentDueDate = membership.RemainingAmount > 0 ? DateTime.UtcNow.AddMonths(1) : null;
            membership.UpdatedAt = DateTime.UtcNow;
            membership.UpdatedBy = request.UpdatedBy;

            await _context.MembersMemberships.ReplaceOneAsync(filter, membership);

            // Log activity
            var activity = new Activity
            {
                ActivityId = _context.GetNextSequenceValue("Activities"),
                ActivityType = "MembershipUpgraded",
                Description = $"Membership upgraded from {oldPlanName} to {newPlan.PlanName}",
                EntityType = "Member",
                EntityId = id,
                RecipientName = memberName,
                RecipientContact = enquiry?.Email,
                MessageContent = $"Plan: {newPlan.PlanName}, Amount: ${newPlan.Price}, Paid: ${request.PaidAmount}",
                IsSuccessful = true,
                PerformedBy = request.UpdatedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Activities.InsertOneAsync(activity);

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
            var filter = Builders<MembersMembership>.Filter.Eq(m => m.MembersMembershipId, id);
            var membersMembership = await _context.MembersMemberships.Find(filter).FirstOrDefaultAsync();
                
            if (membersMembership == null)
            {
                return NotFound();
            }

            // Get member details before deleting
            var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membersMembership.EnquiryId);
            var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

            var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membersMembership.MembershipPlanId);
            var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

            var memberName = $"{enquiry?.FirstName} {enquiry?.LastName}";
            var planName = plan?.PlanName ?? "Unknown Plan";
            var email = enquiry?.Email;

            // Delete the membership
            await _context.MembersMemberships.DeleteOneAsync(filter);

            // Log the deletion activity
            var activity = new Activity
            {
                ActivityId = _context.GetNextSequenceValue("Activities"),
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
            await _context.Activities.InsertOneAsync(activity);

            return NoContent();
        }

        // GET: api/MembersMembership/Stats
        [HttpGet("Stats")]
        public async Task<ActionResult<MembershipStats>> GetMembershipStats()
        {
            var currentDate = DateTime.UtcNow;
            var thirtyDaysFromNow = currentDate.AddDays(30);

            var allMemberships = await _context.MembersMemberships.Find(_ => true).ToListAsync();

            var stats = new MembershipStats
            {
                TotalMemberships = allMemberships.Count,
                ActiveMemberships = allMemberships.Count(mm => mm.IsActive),
                ExpiredMemberships = allMemberships.Count(mm => mm.EndDate < currentDate),
                ExpiringSoon = allMemberships.Count(mm => mm.EndDate >= currentDate && mm.EndDate <= thirtyDaysFromNow),
                PendingPayments = allMemberships.Count(mm => mm.RemainingAmount > 0),
                TotalRevenue = allMemberships.Sum(mm => mm.PaidAmount),
                OutstandingAmount = allMemberships.Sum(mm => mm.RemainingAmount)
            };

            return Ok(stats);
        }

        // GET: api/MembersMembership/ExportToExcel
        [HttpGet("ExportToExcel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var sort = Builders<MembersMembership>.Sort.Descending(mm => mm.CreatedAt);
            var memberships = await _context.MembersMemberships.Find(_ => true).Sort(sort).ToListAsync();

            // Load related data
            var membershipData = new List<(MembersMembership membership, Enquiry enquiry, MembershipPlan plan)>();
            foreach (var membership in memberships)
            {
                var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
                var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

                var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, membership.MembershipPlanId);
                var plan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();

                membershipData.Add((membership, enquiry, plan));
            }

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
                foreach (var (membership, enquiry, plan) in membershipData)
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
                    worksheet.Cells[row, 2].Value = $"{enquiry?.FirstName} {enquiry?.LastName}";
                    worksheet.Cells[row, 3].Value = enquiry?.Email;
                    worksheet.Cells[row, 4].Value = enquiry?.Phone;
                    worksheet.Cells[row, 5].Value = plan?.PlanName;
                    worksheet.Cells[row, 6].Value = membership.DurationInMonths;
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
        public async Task<ActionResult<object>> ToggleMembershipStatus(int id, [FromBody] ToggleStatusRequest request)
        {
            var filter = Builders<MembersMembership>.Filter.Eq(m => m.MembersMembershipId, id);
            var membership = await _context.MembersMemberships.Find(filter).FirstOrDefaultAsync();

            if (membership == null) return NotFound();

            membership.IsInactive = request.IsInactive;
            membership.UpdatedAt = DateTime.UtcNow;
            membership.UpdatedBy = request.UpdatedBy;

            await _context.MembersMemberships.ReplaceOneAsync(filter, membership);

            // Get enquiry for logging
            var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, membership.EnquiryId);
            var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();

            // Log activity
            var memberName = $"{enquiry?.FirstName} {enquiry?.LastName}";
            var activity = new Activity
            {
                ActivityId = _context.GetNextSequenceValue("Activities"),
                ActivityType = request.IsInactive ? "MembershipDeactivated" : "MembershipActivated",
                Description = $"Membership {(request.IsInactive ? "deactivated" : "activated")} for {memberName}",
                EntityType = "Member",
                EntityId = id,
                RecipientName = memberName,
                RecipientContact = enquiry?.Email,
                MessageContent = $"Status changed to: {(request.IsInactive ? "Inactive" : "Active")}",
                IsSuccessful = true,
                PerformedBy = request.UpdatedBy,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Activities.InsertOneAsync(activity);

            return Ok(new { 
                message = $"Membership {(request.IsInactive ? "deactivated" : "activated")} successfully", 
                membership = membership 
            });
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

using Microsoft.AspNetCore.Mvc;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;
using GymManagmentSystem.Models.Enums;
using GymManagmentSystem.Services;
using OfficeOpenXml;
using MongoDB.Driver;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnquiryController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly PdfReceiptService _pdfService;

        public EnquiryController(MongoDbContext context, NotificationService notificationService, PdfReceiptService pdfService)
        {
            _context = context;
            _notificationService = notificationService;
            _pdfService = pdfService;
        }

        // GET: api/Enquiry
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enquiry>>> GetEnquiries()
        {
            var enquiries = await _context.Enquiries.Find(_ => true).ToListAsync();
            return Ok(enquiries);
        }

        // GET: api/Enquiry/Open
        [HttpGet("Open")]
        public async Task<ActionResult<IEnumerable<Enquiry>>> GetOpenEnquiries()
        {
            var filter = Builders<Enquiry>.Filter.Eq(e => e.IsConverted, false);
            var sort = Builders<Enquiry>.Sort.Descending(e => e.CreatedAt);
            var enquiries = await _context.Enquiries.Find(filter).Sort(sort).ToListAsync();
            return Ok(enquiries);
        }

        // GET: api/Enquiry/Closed
        [HttpGet("Closed")]
        public async Task<ActionResult<IEnumerable<Enquiry>>> GetClosedEnquiries()
        {
            var filter = Builders<Enquiry>.Filter.Eq(e => e.IsConverted, true);
            var sort = Builders<Enquiry>.Sort.Descending(e => e.ConvertedDate);
            var enquiries = await _context.Enquiries.Find(filter).Sort(sort).ToListAsync();
            return Ok(enquiries);
        }

        // GET: api/Enquiry/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Enquiry>> GetEnquiry(int id)
        {
            var filter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, id);
            var enquiry = await _context.Enquiries.Find(filter).FirstOrDefaultAsync();

            if (enquiry == null)
            {
                return NotFound();
            }

            return Ok(enquiry);
        }

        // POST: api/Enquiry
        [HttpPost]
        public async Task<ActionResult<Enquiry>> PostEnquiry(Enquiry enquiry)
        {
            // Check for duplicate email
            var emailFilter = Builders<Enquiry>.Filter.Eq(e => e.Email, enquiry.Email);
            var emailCount = await _context.Enquiries.CountDocumentsAsync(emailFilter);
            if (emailCount > 0)
            {
                return BadRequest(new { message = $"Email '{enquiry.Email}' is already registered" });
            }

            // Check for duplicate phone
            var phoneFilter = Builders<Enquiry>.Filter.Eq(e => e.Phone, enquiry.Phone);
            var phoneCount = await _context.Enquiries.CountDocumentsAsync(phoneFilter);
            if (phoneCount > 0)
            {
                return BadRequest(new { message = $"Phone number '{enquiry.Phone}' is already registered" });
            }

            // Generate sequential ID
            enquiry.EnquiryId = _context.GetNextSequenceValue("Enquiries");
            enquiry.CreatedAt = DateTime.UtcNow;
            enquiry.UpdatedAt = DateTime.UtcNow;
            
            try
            {
                await _context.Enquiries.InsertOneAsync(enquiry);
            }
            catch (MongoWriteException)
            {
                // Handle unique constraint violation
                return BadRequest(new { message = "This email or phone number is already in use" });
            }

            // Create history record
            await CreateEnquiryHistory(enquiry, EnquiryAction.Created);

            return CreatedAtAction("GetEnquiry", new { id = enquiry.EnquiryId }, enquiry);
        }

        // PUT: api/Enquiry/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnquiry(int id, Enquiry enquiry)
        {
            if (id != enquiry.EnquiryId)
            {
                return BadRequest();
            }

            // Check for duplicate email (excluding current record)
            var emailFilter = Builders<Enquiry>.Filter.And(
                Builders<Enquiry>.Filter.Eq(e => e.Email, enquiry.Email),
                Builders<Enquiry>.Filter.Ne(e => e.EnquiryId, id)
            );
            var emailCount = await _context.Enquiries.CountDocumentsAsync(emailFilter);
            if (emailCount > 0)
            {
                return BadRequest(new { message = $"Email '{enquiry.Email}' is already registered" });
            }

            // Check for duplicate phone (excluding current record)
            var phoneFilter = Builders<Enquiry>.Filter.And(
                Builders<Enquiry>.Filter.Eq(e => e.Phone, enquiry.Phone),
                Builders<Enquiry>.Filter.Ne(e => e.EnquiryId, id)
            );
            var phoneCount = await _context.Enquiries.CountDocumentsAsync(phoneFilter);
            if (phoneCount > 0)
            {
                return BadRequest(new { message = $"Phone number '{enquiry.Phone}' is already registered" });
            }

            enquiry.UpdatedAt = DateTime.UtcNow;
            
            try
            {
                var filter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, id);
                var result = await _context.Enquiries.ReplaceOneAsync(filter, enquiry);
                
                if (result.MatchedCount == 0)
                {
                    return NotFound();
                }
                
                // Create history record
                await CreateEnquiryHistory(enquiry, EnquiryAction.Updated);
            }
            catch (MongoWriteException)
            {
                return BadRequest(new { message = "This email or phone number is already in use" });
            }

            return NoContent();
        }

        // DELETE: api/Enquiry/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnquiry(int id)
        {
            var filter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, id);
            var enquiry = await _context.Enquiries.Find(filter).FirstOrDefaultAsync();
            
            if (enquiry == null)
            {
                return NotFound();
            }

            // Check if enquiry has been converted to member
            var membershipFilter = Builders<MembersMembership>.Filter.Eq(m => m.EnquiryId, id);
            var hasMembership = await _context.MembersMemberships.CountDocumentsAsync(membershipFilter) > 0;
            if (hasMembership)
            {
                return BadRequest(new { message = "Cannot delete enquiry that has been converted to member. Delete the membership first." });
            }

            // Delete related history records first
            var historyFilter = Builders<EnquiryHistory>.Filter.Eq(h => h.EnquiryId, id);
            await _context.EnquiryHistories.DeleteManyAsync(historyFilter);

            // Create final history record before deletion
            await CreateEnquiryHistory(enquiry, EnquiryAction.Deleted);

            await _context.Enquiries.DeleteOneAsync(filter);

            return NoContent();
        }

        // POST: api/Enquiry/5/ConvertToMember
        [HttpPost("{id}/ConvertToMember")]
        public async Task<ActionResult<MembersMembership>> ConvertToMember(int id, [FromBody] ConvertToMemberRequest request)
        {
            var enquiryFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, id);
            var enquiry = await _context.Enquiries.Find(enquiryFilter).FirstOrDefaultAsync();
            
            if (enquiry == null)
            {
                return NotFound("Enquiry not found");
            }

            // Check if enquiry is already converted
            if (enquiry.IsConverted)
            {
                return BadRequest(new { message = "This enquiry has already been converted to a member" });
            }

            // Check if enquiry already has an active membership
            var membershipFilter = Builders<MembersMembership>.Filter.Eq(m => m.EnquiryId, id);
            var existingMembership = await _context.MembersMemberships.CountDocumentsAsync(membershipFilter) > 0;
            if (existingMembership)
            {
                return BadRequest(new { message = "This enquiry already has a membership" });
            }

            var planFilter = Builders<MembershipPlan>.Filter.Eq(p => p.MembershipPlanId, request.MembershipPlanId);
            var membershipPlan = await _context.MembershipPlans.Find(planFilter).FirstOrDefaultAsync();
            if (membershipPlan == null)
            {
                return NotFound("Membership plan not found");
            }

            var membersMembership = new MembersMembership
            {
                MembersMembershipId = _context.GetNextSequenceValue("MembersMemberships"),
                EnquiryId = enquiry.EnquiryId,
                MembershipPlanId = membershipPlan.MembershipPlanId,
                StartDate = DateTime.UtcNow,
                DurationInMonths = membershipPlan.DurationInMonths,
                TotalAmount = membershipPlan.Price,
                PaidAmount = request.PaidAmount,
                NextPaymentDueDate = DateTime.UtcNow.AddMonths(1),
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            await _context.MembersMemberships.InsertOneAsync(membersMembership);
            
            // Mark enquiry as converted
            enquiry.IsConverted = true;
            enquiry.ConvertedDate = DateTime.UtcNow;
            enquiry.UpdatedAt = DateTime.UtcNow;
            enquiry.UpdatedBy = request.CreatedBy;
            
            var updateFilter = Builders<Enquiry>.Filter.Eq(e => e.EnquiryId, id);
            await _context.Enquiries.ReplaceOneAsync(updateFilter, enquiry);
            
            // Update enquiry history
            await CreateEnquiryHistory(enquiry, EnquiryAction.MembershipTaken, DateTime.UtcNow);

            // Send welcome email and WhatsApp greeting
            var memberName = $"{enquiry.FirstName} {enquiry.LastName}";
            var endDate = membersMembership.StartDate.AddMonths(membershipPlan.DurationInMonths);
            
            // Log conversion activity
            var conversionActivity = new Activity
            {
                ActivityId = _context.GetNextSequenceValue("Activities"),
                ActivityType = "EnquiryConverted",
                Description = $"Enquiry converted to member: {memberName}",
                EntityType = "Enquiry",
                EntityId = enquiry.EnquiryId,
                RecipientName = memberName,
                RecipientContact = enquiry.Email,
                MessageContent = $"Membership Plan: {membershipPlan.PlanName}, Duration: {membershipPlan.DurationInMonths} months, Total Amount: ${membershipPlan.Price:N2}",
                IsSuccessful = true,
                PerformedBy = request.CreatedBy ?? "System",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Activities.InsertOneAsync(conversionActivity);
            
            // Create payment receipt for initial payment if any amount was paid
            PaymentReceipt receipt = null;
            if (request.PaidAmount > 0)
            {
                var receiptCount = await _context.PaymentReceipts.CountDocumentsAsync(_ => true);
                var receiptNumber = $"REC-{DateTime.UtcNow:yyyy}-{(receiptCount + 1):D5}";
                var paymentReceiptId = _context.GetNextSequenceValue("PaymentReceipts");

                receipt = new PaymentReceipt
                {
                    PaymentReceiptId = paymentReceiptId,
                    ReceiptNumber = receiptNumber,
                    MembersMembershipId = membersMembership.MembersMembershipId,
                    AmountPaid = request.PaidAmount,
                    PaymentMethod = "Cash",
                    TransactionId = null,
                    TotalAmount = membershipPlan.Price,
                    PreviousPaid = 0,
                    RemainingAmount = membersMembership.RemainingAmount,
                    Notes = "Initial membership payment",
                    PaymentDate = DateTime.UtcNow,
                    ReceivedBy = request.CreatedBy ?? "Admin",
                    MemberName = memberName,
                    MemberEmail = enquiry.Email,
                    MemberPhone = enquiry.Phone,
                    PlanName = membershipPlan.PlanName,
                    CreatedAt = DateTime.UtcNow,
                    EmailSent = false
                };
                await _context.PaymentReceipts.InsertOneAsync(receipt);
                
                // Generate and save HTML content to database
                receipt.HtmlContent = _pdfService.GenerateReceiptHtmlContent(receipt);
                var receiptFilter = Builders<PaymentReceipt>.Filter.Eq(r => r.PaymentReceiptId, paymentReceiptId);
                await _context.PaymentReceipts.ReplaceOneAsync(receiptFilter, receipt);
                
                // Log initial payment activity
                var paymentActivity = new Activity
                {
                    ActivityId = _context.GetNextSequenceValue("Activities"),
                    ActivityType = "PaymentReceived",
                    Description = $"Initial payment of ${request.PaidAmount:N2} received from {memberName}",
                    EntityType = "Member",
                    EntityId = membersMembership.MembersMembershipId,
                    RecipientName = memberName,
                    RecipientContact = enquiry.Email,
                    MessageContent = $"Receipt: {receiptNumber}, Initial Payment: ${request.PaidAmount:N2}, Total Amount: ${membershipPlan.Price:N2}, Remaining: ${membersMembership.RemainingAmount:N2}, Method: Cash",
                    IsSuccessful = true,
                    PerformedBy = request.CreatedBy ?? "System",
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Activities.InsertOneAsync(paymentActivity);
            }
            
            // Send notifications (async, don't wait - don't block response)
            _ = Task.Run(async () =>
            {
                await _notificationService.SendWelcomeEmailAsync(
                    enquiry.Email,
                    memberName,
                    membershipPlan.PlanName,
                    request.PaidAmount,
                    membersMembership.StartDate,
                    endDate
                );
                
                await _notificationService.SendWhatsAppGreetingAsync(
                    enquiry.Phone,
                    memberName,
                    membershipPlan.PlanName
                );
                
                // Send payment receipt email if payment was made
                if (receipt != null)
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
                        var receiptFilter = Builders<PaymentReceipt>.Filter.Eq(r => r.PaymentReceiptId, receipt.PaymentReceiptId);
                        await _context.PaymentReceipts.ReplaceOneAsync(receiptFilter, receipt);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send receipt email: {ex.Message}");
                    }
                }
            });

            return CreatedAtAction("GetMembersMembership", "MembersMembership", 
                new { id = membersMembership.MembersMembershipId }, membersMembership);
        }

        // GET: api/Enquiry/5/History
        [HttpGet("{id}/History")]
        public async Task<ActionResult<IEnumerable<object>>> GetEnquiryHistory(int id)
        {
            var filter = Builders<EnquiryHistory>.Filter.Eq(eh => eh.EnquiryId, id);
            var sort = Builders<EnquiryHistory>.Sort.Descending(eh => eh.ModifiedAt);
            var history = await _context.EnquiryHistories.Find(filter).Sort(sort).ToListAsync();

            // Convert to response format matching frontend expectations
            var response = history.Select(h => new
            {
                enquiryHistoryId = h.HistoryId,
                enquiryId = h.EnquiryId,
                action = h.ActionTaken.ToString(),
                modifiedBy = h.ModifiedBy,
                modifiedAt = h.ModifiedAt,
                notes = h.ActionTaken == EnquiryAction.MembershipTaken 
                    ? $"Converted to membership on {h.MembershipTakenDate:yyyy-MM-dd}"
                    : h.ActionTaken.ToString()
            });

            return Ok(response);
        }

        // GET: api/Enquiry/ExportToExcel
        [HttpGet("ExportToExcel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var enquiries = await _context.Enquiries.Find(_ => true).ToListAsync();

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Enquiries");

                // Add headers
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "First Name";
                worksheet.Cells[1, 3].Value = "Last Name";
                worksheet.Cells[1, 4].Value = "Email";
                worksheet.Cells[1, 5].Value = "Phone";
                worksheet.Cells[1, 6].Value = "WhatsApp";
                worksheet.Cells[1, 7].Value = "Address";
                worksheet.Cells[1, 8].Value = "City";
                worksheet.Cells[1, 9].Value = "Gender";
                worksheet.Cells[1, 10].Value = "Date of Birth";
                worksheet.Cells[1, 11].Value = "Occupation";
                worksheet.Cells[1, 12].Value = "Created By";
                worksheet.Cells[1, 13].Value = "Created At";

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 13])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Add data
                int row = 2;
                foreach (var enquiry in enquiries)
                {
                    worksheet.Cells[row, 1].Value = enquiry.EnquiryId;
                    worksheet.Cells[row, 2].Value = enquiry.FirstName;
                    worksheet.Cells[row, 3].Value = enquiry.LastName;
                    worksheet.Cells[row, 4].Value = enquiry.Email;
                    worksheet.Cells[row, 5].Value = enquiry.Phone;
                    worksheet.Cells[row, 6].Value = enquiry.IsWhatsappNumber ? "Yes" : "No";
                    worksheet.Cells[row, 7].Value = enquiry.Address;
                    worksheet.Cells[row, 8].Value = enquiry.City;
                    worksheet.Cells[row, 9].Value = enquiry.Gender;
                    worksheet.Cells[row, 10].Value = enquiry.DateOfBirth.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 11].Value = enquiry.Occupation;
                    worksheet.Cells[row, 12].Value = enquiry.Createdby;
                    worksheet.Cells[row, 13].Value = enquiry.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Generate file
                var fileBytes = package.GetAsByteArray();
                var fileName = $"Enquiries_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        private async Task CreateEnquiryHistory(Enquiry enquiry, EnquiryAction action, DateTime? membershipDate = null)
        {
            var history = new EnquiryHistory
            {
                HistoryId = _context.GetNextSequenceValue("EnquiryHistories"),
                EnquiryId = enquiry.EnquiryId,
                FirstName = enquiry.FirstName,
                LastName = enquiry.LastName,
                Email = enquiry.Email,
                Phone = enquiry.Phone,
                IsWhatsappNumber = enquiry.IsWhatsappNumber,
                Address = enquiry.Address,
                City = enquiry.City,
                Gender = enquiry.Gender,
                DateOfBirth = enquiry.DateOfBirth,
                Occupation = enquiry.Occupation,
                ActionTaken = action,
                MembershipTakenDate = membershipDate,
                ModifiedBy = enquiry.UpdatedBy ?? enquiry.Createdby,
                ModifiedAt = DateTime.UtcNow
            };

            await _context.EnquiryHistories.InsertOneAsync(history);
        }
    }

    public class ConvertToMemberRequest
    {
        public int MembershipPlanId { get; set; }
        public decimal PaidAmount { get; set; }
        public string CreatedBy { get; set; }
    }
}

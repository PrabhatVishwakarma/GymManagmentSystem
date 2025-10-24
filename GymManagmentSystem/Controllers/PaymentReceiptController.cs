using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;
using GymManagmentSystem.Services;
using System.Security.Claims;

namespace GymManagmentSystem.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentReceiptController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PdfReceiptService _pdfService;

        public PaymentReceiptController(AppDbContext context, PdfReceiptService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
        }

        private string GetCurrentUserName()
        {
            var firstName = User.FindFirst("FirstName")?.Value ?? "";
            var lastName = User.FindFirst("LastName")?.Value ?? "";
            return $"{firstName} {lastName}".Trim();
        }

        // GET: api/PaymentReceipt/member/{membershipId}
        [HttpGet("member/{membershipId}")]
        public async Task<ActionResult<IEnumerable<PaymentReceipt>>> GetMemberReceipts(int membershipId)
        {
            var receipts = await _context.PaymentReceipts
                .Where(r => r.MembersMembershipId == membershipId)
                .OrderByDescending(r => r.PaymentDate)
                .ToListAsync();

            return Ok(receipts);
        }

        // GET: api/PaymentReceipt/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentReceipt>> GetReceipt(int id)
        {
            var receipt = await _context.PaymentReceipts.FindAsync(id);

            if (receipt == null)
            {
                return NotFound();
            }

            return Ok(receipt);
        }

        // GET: api/PaymentReceipt/{id}/download
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadReceipt(int id)
        {
            var receipt = await _context.PaymentReceipts.FindAsync(id);

            if (receipt == null)
            {
                return NotFound();
            }

            var userName = GetCurrentUserName();

            try
            {
                Console.WriteLine($"Attempting to generate PDF for receipt {receipt.ReceiptNumber}");
                
                // Generate PDF from receipt
                var pdfBytes = _pdfService.GenerateReceiptPdf(receipt);
                
                Console.WriteLine($"PDF generated successfully! Size: {pdfBytes.Length} bytes");
                
                // Log download activity
                var activity = new Activity
                {
                    ActivityType = "ReceiptDownloaded",
                    Description = $"Downloaded payment receipt {receipt.ReceiptNumber} for {receipt.MemberName} (₹{receipt.AmountPaid:N2})",
                    EntityType = "PaymentReceipt",
                    EntityId = receipt.PaymentReceiptId,
                    PerformedBy = userName,
                    CreatedAt = DateTime.UtcNow,
                    IsSuccessful = true,
                    MessageContent = $"Format: PDF, Amount: ₹{receipt.AmountPaid:N2}",
                    RecipientName = receipt.MemberName,
                    RecipientContact = receipt.MemberEmail
                };
                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();
                
                return File(pdfBytes, "application/pdf", $"Receipt_{receipt.ReceiptNumber}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ PDF generation failed!");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
                }
                
                // Fallback to HTML if PDF generation fails
                Console.WriteLine("Falling back to HTML download...");
                var htmlContent = receipt.HtmlContent;
                if (string.IsNullOrEmpty(htmlContent))
                {
                    htmlContent = _pdfService.GenerateReceiptHtmlContent(receipt);
                }
                
                // Log download activity (HTML fallback)
                var activity = new Activity
                {
                    ActivityType = "ReceiptDownloaded",
                    Description = $"Downloaded payment receipt {receipt.ReceiptNumber} for {receipt.MemberName} (₹{receipt.AmountPaid:N2}) - HTML fallback",
                    EntityType = "PaymentReceipt",
                    EntityId = receipt.PaymentReceiptId,
                    PerformedBy = userName,
                    CreatedAt = DateTime.UtcNow,
                    IsSuccessful = true,
                    MessageContent = $"Format: HTML (PDF generation failed), Amount: ₹{receipt.AmountPaid:N2}",
                    RecipientName = receipt.MemberName,
                    RecipientContact = receipt.MemberEmail,
                    ErrorMessage = ex.Message
                };
                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();
                
                var htmlBytes = System.Text.Encoding.UTF8.GetBytes(htmlContent);
                return File(htmlBytes, "text/html", $"Receipt_{receipt.ReceiptNumber}.html");
            }
        }

        // GET: api/PaymentReceipt/{id}/html
        [HttpGet("{id}/html")]
        public async Task<IActionResult> GetReceiptHtml(int id)
        {
            var receipt = await _context.PaymentReceipts.FindAsync(id);

            if (receipt == null)
            {
                return NotFound();
            }

            var userName = GetCurrentUserName();

            // Use stored HTML content if available, otherwise generate it
            var htmlContent = receipt.HtmlContent;
            if (string.IsNullOrEmpty(htmlContent))
            {
                htmlContent = _pdfService.GenerateReceiptHtmlContent(receipt);
                
                // Save generated HTML for future use
                receipt.HtmlContent = htmlContent;
                _context.PaymentReceipts.Update(receipt);
                await _context.SaveChangesAsync();
            }
            
            // Log view activity
            var activity = new Activity
            {
                ActivityType = "ReceiptViewed",
                Description = $"Viewed payment receipt {receipt.ReceiptNumber} for {receipt.MemberName} (₹{receipt.AmountPaid:N2})",
                EntityType = "PaymentReceipt",
                EntityId = receipt.PaymentReceiptId,
                PerformedBy = userName,
                CreatedAt = DateTime.UtcNow,
                IsSuccessful = true,
                MessageContent = $"Viewed in browser, Amount: ₹{receipt.AmountPaid:N2}",
                RecipientName = receipt.MemberName,
                RecipientContact = receipt.MemberEmail
            };
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            
            return Content(htmlContent, "text/html");
        }

        // GET: api/PaymentReceipt/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<PaymentReceipt>>> GetAllReceipts()
        {
            var receipts = await _context.PaymentReceipts
                .OrderByDescending(r => r.PaymentDate)
                .ToListAsync();

            return Ok(receipts);
        }

        // DELETE: api/PaymentReceipt/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReceipt(int id)
        {
            var receipt = await _context.PaymentReceipts.FindAsync(id);
            if (receipt == null)
            {
                return NotFound();
            }

            _context.PaymentReceipts.Remove(receipt);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}


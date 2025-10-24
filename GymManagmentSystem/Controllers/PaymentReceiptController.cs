using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;
using GymManagmentSystem.Services;

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

            try
            {
                // Generate PDF from receipt
                var pdfBytes = _pdfService.GenerateReceiptPdf(receipt);
                return File(pdfBytes, "application/pdf", $"Receipt_{receipt.ReceiptNumber}.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF generation failed: {ex.Message}");
                
                // Fallback to HTML if PDF generation fails
                var htmlContent = receipt.HtmlContent;
                if (string.IsNullOrEmpty(htmlContent))
                {
                    htmlContent = _pdfService.GenerateReceiptHtmlContent(receipt);
                }
                
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


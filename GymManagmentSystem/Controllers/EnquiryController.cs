using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;
using GymManagmentSystem.Models.Enums;
using OfficeOpenXml;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnquiryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnquiryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Enquiry
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enquiry>>> GetEnquiries()
        {
            return await _context.Enquiries.ToListAsync();
        }

        // GET: api/Enquiry/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Enquiry>> GetEnquiry(int id)
        {
            var enquiry = await _context.Enquiries.FindAsync(id);

            if (enquiry == null)
            {
                return NotFound();
            }

            return enquiry;
        }

        // POST: api/Enquiry
        [HttpPost]
        public async Task<ActionResult<Enquiry>> PostEnquiry(Enquiry enquiry)
        {
            enquiry.CreatedAt = DateTime.UtcNow;
            enquiry.UpdatedAt = DateTime.UtcNow;
            
            _context.Enquiries.Add(enquiry);
            await _context.SaveChangesAsync();

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

            enquiry.UpdatedAt = DateTime.UtcNow;
            _context.Entry(enquiry).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                
                // Create history record
                await CreateEnquiryHistory(enquiry, EnquiryAction.Updated);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnquiryExists(id))
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

        // DELETE: api/Enquiry/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnquiry(int id)
        {
            var enquiry = await _context.Enquiries.FindAsync(id);
            if (enquiry == null)
            {
                return NotFound();
            }

            // Check if enquiry has been converted to member
            var hasMembership = await _context.MembersMemberships.AnyAsync(m => m.EnquiryId == id);
            if (hasMembership)
            {
                return BadRequest(new { message = "Cannot delete enquiry that has been converted to member. Delete the membership first." });
            }

            // Delete related history records first
            var historyRecords = await _context.EnquiryHistories.Where(h => h.EnquiryId == id).ToListAsync();
            _context.EnquiryHistories.RemoveRange(historyRecords);

            // Create final history record before deletion
            await CreateEnquiryHistory(enquiry, EnquiryAction.Deleted);

            _context.Enquiries.Remove(enquiry);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Enquiry/5/ConvertToMember
        [HttpPost("{id}/ConvertToMember")]
        public async Task<ActionResult<MembersMembership>> ConvertToMember(int id, [FromBody] ConvertToMemberRequest request)
        {
            var enquiry = await _context.Enquiries.FindAsync(id);
            if (enquiry == null)
            {
                return NotFound("Enquiry not found");
            }

            var membershipPlan = await _context.MembershipPlans.FindAsync(request.MembershipPlanId);
            if (membershipPlan == null)
            {
                return NotFound("Membership plan not found");
            }

            var membersMembership = new MembersMembership
            {
                EnquiryId = enquiry.EnquiryId,
                MembershipPlanId = membershipPlan.MembershipPlanId,
                StartDate = DateTime.UtcNow,
                TotalAmount = membershipPlan.Price,
                PaidAmount = request.PaidAmount,
                NextPaymentDueDate = DateTime.UtcNow.AddMonths(1),
                CreatedBy = request.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.MembersMemberships.Add(membersMembership);
            
            // Update enquiry history
            await CreateEnquiryHistory(enquiry, EnquiryAction.MembershipTaken, DateTime.UtcNow);

            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMembersMembership", "MembersMembership", 
                new { id = membersMembership.MembersMembershipId }, membersMembership);
        }

        // GET: api/Enquiry/5/History
        [HttpGet("{id}/History")]
        public async Task<ActionResult<IEnumerable<EnquiryHistory>>> GetEnquiryHistory(int id)
        {
            var history = await _context.EnquiryHistories
                .Where(eh => eh.EnquiryId == id)
                .OrderByDescending(eh => eh.ModifiedAt)
                .ToListAsync();

            return history;
        }

        // GET: api/Enquiry/ExportToExcel
        [HttpGet("ExportToExcel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var enquiries = await _context.Enquiries.ToListAsync();

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

        private bool EnquiryExists(int id)
        {
            return _context.Enquiries.Any(e => e.EnquiryId == id);
        }

        private async Task CreateEnquiryHistory(Enquiry enquiry, EnquiryAction action, DateTime? membershipDate = null)
        {
            var history = new EnquiryHistory
            {
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

            _context.EnquiryHistories.Add(history);
            await _context.SaveChangesAsync();
        }
    }

    public class ConvertToMemberRequest
    {
        public int MembershipPlanId { get; set; }
        public decimal PaidAmount { get; set; }
        public string CreatedBy { get; set; }
    }
}

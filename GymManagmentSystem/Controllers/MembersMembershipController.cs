using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersMembershipController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MembersMembershipController(AppDbContext context)
        {
            _context = context;
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
            var membersMembership = await _context.MembersMemberships.FindAsync(id);
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

            membersMembership.PaidAmount += paymentRequest.Amount;
            membersMembership.UpdatedAt = DateTime.UtcNow;

            // Update next payment due date if there's still remaining amount
            if (membersMembership.RemainingAmount > 0)
            {
                membersMembership.NextPaymentDueDate = DateTime.UtcNow.AddMonths(1);
            }

            await _context.SaveChangesAsync();

            var response = new PaymentResponse
            {
                MembersMembershipId = membersMembership.MembersMembershipId,
                PaymentAmount = paymentRequest.Amount,
                RemainingAmount = membersMembership.RemainingAmount,
                IsFullyPaid = membersMembership.RemainingAmount <= 0,
                NextPaymentDueDate = membersMembership.NextPaymentDueDate
            };

            return Ok(response);
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

        // DELETE: api/MembersMembership/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMembersMembership(int id)
        {
            var membersMembership = await _context.MembersMemberships.FindAsync(id);
            if (membersMembership == null)
            {
                return NotFound();
            }

            _context.MembersMemberships.Remove(membersMembership);
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

        private bool MembersMembershipExists(int id)
        {
            return _context.MembersMemberships.Any(e => e.MembersMembershipId == id);
        }
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
    }

    public class PaymentResponse
    {
        public int MembersMembershipId { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool IsFullyPaid { get; set; }
        public DateTime? NextPaymentDueDate { get; set; }
    }

    public class RenewalRequest
    {
        public decimal PaidAmount { get; set; }
        public string UpdatedBy { get; set; }
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

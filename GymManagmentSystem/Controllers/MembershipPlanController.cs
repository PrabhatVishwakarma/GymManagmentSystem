using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipPlanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MembershipPlanController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/MembershipPlan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembershipPlan>>> GetMembershipPlans()
        {
            return await _context.MembershipPlans
                .Where(mp => mp.IsActive)
                .OrderBy(mp => mp.PlanName)
                .ToListAsync();
        }

        // GET: api/MembershipPlan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MembershipPlan>> GetMembershipPlan(int id)
        {
            var membershipPlan = await _context.MembershipPlans.FindAsync(id);

            if (membershipPlan == null)
            {
                return NotFound();
            }

            return membershipPlan;
        }

        // GET: api/MembershipPlan/Active
        [HttpGet("Active")]
        public async Task<ActionResult<IEnumerable<MembershipPlan>>> GetActiveMembershipPlans()
        {
            return await _context.MembershipPlans
                .Where(mp => mp.IsActive)
                .OrderBy(mp => mp.Price)
                .ToListAsync();
        }

        // GET: api/MembershipPlan/ByType/{planType}
        [HttpGet("ByType/{planType}")]
        public async Task<ActionResult<IEnumerable<MembershipPlan>>> GetMembershipPlansByType(string planType)
        {
            return await _context.MembershipPlans
                .Where(mp => mp.PlanType.ToLower() == planType.ToLower() && mp.IsActive)
                .OrderBy(mp => mp.Price)
                .ToListAsync();
        }

        // POST: api/MembershipPlan
        [HttpPost]
        public async Task<ActionResult<MembershipPlan>> PostMembershipPlan(MembershipPlan membershipPlan)
        {
            membershipPlan.CreatedAt = DateTime.UtcNow;
            membershipPlan.UpdatedAt = DateTime.UtcNow;
            
            _context.MembershipPlans.Add(membershipPlan);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMembershipPlan", new { id = membershipPlan.MembershipPlanId }, membershipPlan);
        }

        // PUT: api/MembershipPlan/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMembershipPlan(int id, MembershipPlan membershipPlan)
        {
            if (id != membershipPlan.MembershipPlanId)
            {
                return BadRequest();
            }

            membershipPlan.UpdatedAt = DateTime.UtcNow;
            _context.Entry(membershipPlan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MembershipPlanExists(id))
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

        // DELETE: api/MembershipPlan/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMembershipPlan(int id)
        {
            var membershipPlan = await _context.MembershipPlans.FindAsync(id);
            if (membershipPlan == null)
            {
                return NotFound();
            }

            // Soft delete by setting IsActive to false
            membershipPlan.IsActive = false;
            membershipPlan.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/MembershipPlan/5/Activate
        [HttpPut("{id}/Activate")]
        public async Task<IActionResult> ActivateMembershipPlan(int id)
        {
            var membershipPlan = await _context.MembershipPlans.FindAsync(id);
            if (membershipPlan == null)
            {
                return NotFound();
            }

            membershipPlan.IsActive = true;
            membershipPlan.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/MembershipPlan/5/Members
        [HttpGet("{id}/Members")]
        public async Task<ActionResult<IEnumerable<MembersMembership>>> GetMembershipPlanMembers(int id)
        {
            var members = await _context.MembersMemberships
                .Include(mm => mm.Enquiry)
                .Include(mm => mm.MembershipPlan)
                .Where(mm => mm.MembershipPlanId == id)
                .ToListAsync();

            return members;
        }

        // GET: api/MembershipPlan/Stats
        [HttpGet("Stats")]
        public async Task<ActionResult<MembershipPlanStats>> GetMembershipPlanStats()
        {
            var stats = new MembershipPlanStats
            {
                TotalPlans = await _context.MembershipPlans.CountAsync(),
                ActivePlans = await _context.MembershipPlans.CountAsync(mp => mp.IsActive),
                TotalMembers = await _context.MembersMemberships.CountAsync(),
                ActiveMembers = await _context.MembersMemberships.CountAsync(mm => mm.IsActive),
                TotalRevenue = await _context.MembersMemberships.SumAsync(mm => mm.PaidAmount)
            };

            return stats;
        }

        private bool MembershipPlanExists(int id)
        {
            return _context.MembershipPlans.Any(e => e.MembershipPlanId == id);
        }
    }

    public class MembershipPlanStats
    {
        public int TotalPlans { get; set; }
        public int ActivePlans { get; set; }
        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}

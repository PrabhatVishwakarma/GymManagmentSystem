using Microsoft.AspNetCore.Mvc;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;
using MongoDB.Driver;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipPlanController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public MembershipPlanController(MongoDbContext context)
        {
            _context = context;
        }

        // GET: api/MembershipPlan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MembershipPlan>>> GetMembershipPlans()
        {
            var filter = Builders<MembershipPlan>.Filter.Eq(mp => mp.IsActive, true);
            var sort = Builders<MembershipPlan>.Sort.Ascending(mp => mp.PlanName);
            var plans = await _context.MembershipPlans.Find(filter).Sort(sort).ToListAsync();
            return Ok(plans);
        }

        // GET: api/MembershipPlan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MembershipPlan>> GetMembershipPlan(int id)
        {
            var filter = Builders<MembershipPlan>.Filter.Eq(mp => mp.MembershipPlanId, id);
            var membershipPlan = await _context.MembershipPlans.Find(filter).FirstOrDefaultAsync();

            if (membershipPlan == null)
            {
                return NotFound();
            }

            return Ok(membershipPlan);
        }

        // GET: api/MembershipPlan/Active
        [HttpGet("Active")]
        public async Task<ActionResult<IEnumerable<MembershipPlan>>> GetActiveMembershipPlans()
        {
            var filter = Builders<MembershipPlan>.Filter.Eq(mp => mp.IsActive, true);
            var sort = Builders<MembershipPlan>.Sort.Ascending(mp => mp.Price);
            var plans = await _context.MembershipPlans.Find(filter).Sort(sort).ToListAsync();
            return Ok(plans);
        }

        // GET: api/MembershipPlan/ByType/{planType}
        [HttpGet("ByType/{planType}")]
        public async Task<ActionResult<IEnumerable<MembershipPlan>>> GetMembershipPlansByType(string planType)
        {
            var filter = Builders<MembershipPlan>.Filter.And(
                Builders<MembershipPlan>.Filter.Regex(mp => mp.PlanType, new MongoDB.Bson.BsonRegularExpression(planType, "i")),
                Builders<MembershipPlan>.Filter.Eq(mp => mp.IsActive, true)
            );
            var sort = Builders<MembershipPlan>.Sort.Ascending(mp => mp.Price);
            var plans = await _context.MembershipPlans.Find(filter).Sort(sort).ToListAsync();
            return Ok(plans);
        }

        // POST: api/MembershipPlan
        [HttpPost]
        public async Task<ActionResult<MembershipPlan>> PostMembershipPlan(MembershipPlan membershipPlan)
        {
            membershipPlan.MembershipPlanId = _context.GetNextSequenceValue("MembershipPlans");
            membershipPlan.CreatedAt = DateTime.UtcNow;
            membershipPlan.UpdatedAt = DateTime.UtcNow;
            
            await _context.MembershipPlans.InsertOneAsync(membershipPlan);

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
            
            var filter = Builders<MembershipPlan>.Filter.Eq(mp => mp.MembershipPlanId, id);
            var result = await _context.MembershipPlans.ReplaceOneAsync(filter, membershipPlan);

            if (result.MatchedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/MembershipPlan/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMembershipPlan(int id)
        {
            var filter = Builders<MembershipPlan>.Filter.Eq(mp => mp.MembershipPlanId, id);
            var membershipPlan = await _context.MembershipPlans.Find(filter).FirstOrDefaultAsync();
            
            if (membershipPlan == null)
            {
                return NotFound();
            }

            // Soft delete by setting IsActive to false
            membershipPlan.IsActive = false;
            membershipPlan.UpdatedAt = DateTime.UtcNow;
            
            await _context.MembershipPlans.ReplaceOneAsync(filter, membershipPlan);

            return NoContent();
        }

        // PUT: api/MembershipPlan/5/Activate
        [HttpPut("{id}/Activate")]
        public async Task<IActionResult> ActivateMembershipPlan(int id)
        {
            var filter = Builders<MembershipPlan>.Filter.Eq(mp => mp.MembershipPlanId, id);
            var membershipPlan = await _context.MembershipPlans.Find(filter).FirstOrDefaultAsync();
            
            if (membershipPlan == null)
            {
                return NotFound();
            }

            membershipPlan.IsActive = true;
            membershipPlan.UpdatedAt = DateTime.UtcNow;
            
            await _context.MembershipPlans.ReplaceOneAsync(filter, membershipPlan);

            return NoContent();
        }

        // GET: api/MembershipPlan/5/Members
        [HttpGet("{id}/Members")]
        public async Task<ActionResult<IEnumerable<object>>> GetMembershipPlanMembers(int id)
        {
            // Get members with this plan
            var membershipFilter = Builders<MembersMembership>.Filter.Eq(mm => mm.MembershipPlanId, id);
            var memberships = await _context.MembersMemberships.Find(membershipFilter).ToListAsync();

            // Get related enquiries and plans for each membership
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
                    isActive = membership.IsActive,
                    enquiry = enquiry,
                    membershipPlan = plan,
                    createdAt = membership.CreatedAt
                });
            }

            return Ok(result);
        }

        // GET: api/MembershipPlan/Stats
        [HttpGet("Stats")]
        public async Task<ActionResult<MembershipPlanStats>> GetMembershipPlanStats()
        {
            var totalPlans = await _context.MembershipPlans.CountDocumentsAsync(_ => true);
            var activeFilter = Builders<MembershipPlan>.Filter.Eq(mp => mp.IsActive, true);
            var activePlans = await _context.MembershipPlans.CountDocumentsAsync(activeFilter);
            
            var totalMembers = await _context.MembersMemberships.CountDocumentsAsync(_ => true);
            
            // Count active members
            var memberships = await _context.MembersMemberships.Find(_ => true).ToListAsync();
            var activeMembers = memberships.Count(mm => mm.IsActive);
            
            // Sum total revenue
            var totalRevenue = memberships.Sum(mm => mm.PaidAmount);

            var stats = new MembershipPlanStats
            {
                TotalPlans = (int)totalPlans,
                ActivePlans = (int)activePlans,
                TotalMembers = (int)totalMembers,
                ActiveMembers = activeMembers,
                TotalRevenue = totalRevenue
            };

            return Ok(stats);
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

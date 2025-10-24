using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ActivityController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Activity
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivities(
            [FromQuery] int? limit = 100,
            [FromQuery] string activityType = null,
            [FromQuery] string entityType = null)
        {
            var query = _context.Activities.AsQueryable();

            if (!string.IsNullOrEmpty(activityType))
            {
                query = query.Where(a => a.ActivityType == activityType);
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                query = query.Where(a => a.EntityType == entityType);
            }

            var activities = await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit ?? 100)
                .ToListAsync();

            return Ok(activities);
        }

        // GET: api/Activity/Recent
        [HttpGet("Recent")]
        public async Task<ActionResult<IEnumerable<Activity>>> GetRecentActivities([FromQuery] int limit = 50)
        {
            var activities = await _context.Activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();

            return Ok(activities);
        }

        // GET: api/Activity/ByEntity/Member/5
        [HttpGet("ByEntity/{entityType}/{entityId}")]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivitiesByEntity(string entityType, int entityId)
        {
            var activities = await _context.Activities
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return Ok(activities);
        }

        // GET: api/Activity/Stats
        [HttpGet("Stats")]
        public async Task<ActionResult<object>> GetActivityStats()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddDays(-30);

            var stats = new
            {
                TotalActivities = await _context.Activities.CountAsync(),
                TodayActivities = await _context.Activities.CountAsync(a => a.CreatedAt >= today),
                WeekActivities = await _context.Activities.CountAsync(a => a.CreatedAt >= weekAgo),
                MonthActivities = await _context.Activities.CountAsync(a => a.CreatedAt >= monthAgo),
                EmailsSent = await _context.Activities.CountAsync(a => a.ActivityType == "Email" && a.IsSuccessful),
                WhatsAppSent = await _context.Activities.CountAsync(a => a.ActivityType == "WhatsApp" && a.IsSuccessful),
                FailedActivities = await _context.Activities.CountAsync(a => !a.IsSuccessful),
                ActivityTypes = await _context.Activities
                    .GroupBy(a => a.ActivityType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToListAsync()
            };

            return Ok(stats);
        }

        // POST: api/Activity (Manual activity logging)
        [HttpPost]
        public async Task<ActionResult<Activity>> CreateActivity(Activity activity)
        {
            activity.CreatedAt = DateTime.UtcNow;
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetActivities", new { id = activity.ActivityId }, activity);
        }

        // DELETE: api/Activity/5 (Admin only - clear old logs)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return NotFound();

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Activity/ClearOld (Admin - clear logs older than X days)
        [HttpDelete("ClearOld")]
        public async Task<IActionResult> ClearOldActivities([FromQuery] int daysOld = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldActivities = await _context.Activities
                .Where(a => a.CreatedAt < cutoffDate)
                .ToListAsync();

            _context.Activities.RemoveRange(oldActivities);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Deleted {oldActivities.Count} activities older than {daysOld} days" });
        }
    }
}


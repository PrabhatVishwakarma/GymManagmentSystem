using Microsoft.AspNetCore.Mvc;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;
using MongoDB.Driver;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public ActivityController(MongoDbContext context)
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
            var filterBuilder = Builders<Activity>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(activityType))
            {
                filter = filter & filterBuilder.Eq(a => a.ActivityType, activityType);
            }

            if (!string.IsNullOrEmpty(entityType))
            {
                filter = filter & filterBuilder.Eq(a => a.EntityType, entityType);
            }

            var sort = Builders<Activity>.Sort.Descending(a => a.CreatedAt);
            var activities = await _context.Activities
                .Find(filter)
                .Sort(sort)
                .Limit(limit ?? 100)
                .ToListAsync();

            return Ok(activities);
        }

        // GET: api/Activity/Recent
        [HttpGet("Recent")]
        public async Task<ActionResult<IEnumerable<Activity>>> GetRecentActivities([FromQuery] int limit = 50)
        {
            var sort = Builders<Activity>.Sort.Descending(a => a.CreatedAt);
            var activities = await _context.Activities
                .Find(_ => true)
                .Sort(sort)
                .Limit(limit)
                .ToListAsync();

            return Ok(activities);
        }

        // GET: api/Activity/ByEntity/Member/5
        [HttpGet("ByEntity/{entityType}/{entityId}")]
        public async Task<ActionResult<IEnumerable<Activity>>> GetActivitiesByEntity(string entityType, int entityId)
        {
            var filter = Builders<Activity>.Filter.And(
                Builders<Activity>.Filter.Eq(a => a.EntityType, entityType),
                Builders<Activity>.Filter.Eq(a => a.EntityId, entityId)
            );
            
            var sort = Builders<Activity>.Sort.Descending(a => a.CreatedAt);
            var activities = await _context.Activities.Find(filter).Sort(sort).ToListAsync();

            return Ok(activities);
        }

        // GET: api/Activity/Stats
        [HttpGet("Stats")]
        public async Task<ActionResult<object>> GetActivityStats()
        {
            var today = DateTime.UtcNow.Date;
            var weekAgo = today.AddDays(-7);
            var monthAgo = today.AddDays(-30);

            // Load all activities for stats calculation
            var allActivities = await _context.Activities.Find(_ => true).ToListAsync();

            var stats = new
            {
                TotalActivities = allActivities.Count,
                TodayActivities = allActivities.Count(a => a.CreatedAt >= today),
                WeekActivities = allActivities.Count(a => a.CreatedAt >= weekAgo),
                MonthActivities = allActivities.Count(a => a.CreatedAt >= monthAgo),
                EmailsSent = allActivities.Count(a => a.ActivityType == "Email" && a.IsSuccessful),
                WhatsAppSent = allActivities.Count(a => a.ActivityType == "WhatsApp" && a.IsSuccessful),
                FailedActivities = allActivities.Count(a => !a.IsSuccessful),
                ActivityTypes = allActivities
                    .GroupBy(a => a.ActivityType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToList()
            };

            return Ok(stats);
        }

        // POST: api/Activity (Manual activity logging)
        [HttpPost]
        public async Task<ActionResult<Activity>> CreateActivity(Activity activity)
        {
            activity.ActivityId = _context.GetNextSequenceValue("Activities");
            activity.CreatedAt = DateTime.UtcNow;
            
            await _context.Activities.InsertOneAsync(activity);

            return CreatedAtAction("GetActivities", new { id = activity.ActivityId }, activity);
        }

        // DELETE: api/Activity/5 (Admin only - clear old logs)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            var filter = Builders<Activity>.Filter.Eq(a => a.ActivityId, id);
            var result = await _context.Activities.DeleteOneAsync(filter);
            
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Activity/ClearOld (Admin - clear logs older than X days)
        [HttpDelete("ClearOld")]
        public async Task<IActionResult> ClearOldActivities([FromQuery] int daysOld = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var filter = Builders<Activity>.Filter.Lt(a => a.CreatedAt, cutoffDate);
            
            var result = await _context.Activities.DeleteManyAsync(filter);

            return Ok(new { message = $"Deleted {result.DeletedCount} activities older than {daysOld} days" });
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GymManagmentSystem.Data;
using GymManagmentSystem.Models;
using GymManagmentSystem.Models.Enums;
using System.Security.Claims;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public UserController(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _userManager.Users.ToListAsync();
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: api/User/ByEmail/{email}
        [HttpGet("ByEmail/{email}")]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/User/Register
        [HttpPost("Register")]
        public async Task<ActionResult<User>> RegisterUser([FromBody] RegisterUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Parse date if it's a string
                DateTime dateOfBirth;
                if (request.DateOfBirth.HasValue)
                {
                    dateOfBirth = request.DateOfBirth.Value;
                }
                else if (!string.IsNullOrEmpty(request.DateOfBirthString))
                {
                    if (!DateTime.TryParse(request.DateOfBirthString, out dateOfBirth))
                    {
                        return BadRequest(new { message = "Invalid date format for DateOfBirth" });
                    }
                }
                else
                {
                    return BadRequest(new { message = "DateOfBirth is required" });
                }

                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Gender = request.Gender,
                    Address = request.Address,
                    DateOfBirth = dateOfBirth,
                    Occupation = request.Occupation,
                    CreatedBy = request.CreatedBy ?? "System",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (result.Succeeded)
                {
                    // Add to role if specified
                    if (!string.IsNullOrEmpty(request.Role))
                    {
                        await _userManager.AddToRoleAsync(user, request.Role);
                    }

                    return CreatedAtAction("GetUser", new { id = user.Id }, user);
                }

                return BadRequest(new { errors = result.Errors, message = "User creation failed" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, innerMessage = ex.InnerException?.Message });
            }
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Gender = request.Gender;
            user.Address = request.Address;
            user.DateOfBirth = request.DateOfBirth;
            user.Occupation = request.Occupation;
            user.UpdatedBy = request.UpdatedBy;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // POST: api/User/5/ChangePassword
        [HttpPost("{id}/ChangePassword")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // POST: api/User/5/ResetPassword
        [HttpPost("{id}/ResetPassword")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetPasswordRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // POST: api/User/5/AssignRole
        [HttpPost("{id}/AssignRole")]
        public async Task<IActionResult> AssignRole(string id, [FromBody] AssignRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.AddToRoleAsync(user, request.Role);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // DELETE: api/User/5/RemoveRole
        [HttpDelete("{id}/RemoveRole")]
        public async Task<IActionResult> RemoveRole(string id, [FromBody] RemoveRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.RemoveFromRoleAsync(user, request.Role);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // GET: api/User/5/Roles
        [HttpGet("{id}/Roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        // GET: api/User/Stats
        [HttpGet("Stats")]
        public async Task<ActionResult<UserStats>> GetUserStats()
        {
            var stats = new UserStats
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                UsersCreatedThisMonth = await _userManager.Users
                    .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddMonths(-1)),
                UsersCreatedThisYear = await _userManager.Users
                    .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddYears(-1))
            };

            return stats;
        }

        // GET: api/User/Profile
        [HttpGet("Profile")]
        public async Task<ActionResult<User>> GetCurrentUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // GET: api/User/AvailableRoles
        [HttpGet("AvailableRoles")]
        public ActionResult<IEnumerable<string>> GetAvailableRoles()
        {
            var roles = Enum.GetNames(typeof(UserRole)).ToList();
            return Ok(roles);
        }
    }

    public class RegisterUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string DateOfBirthString { get; set; }
        public string Occupation { get; set; }
        public string Role { get; set; }
        public string CreatedBy { get; set; }
    }

    public class UpdateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Occupation { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; }
    }

    public class AssignRoleRequest
    {
        public string Role { get; set; }
    }

    public class RemoveRoleRequest
    {
        public string Role { get; set; }
    }

    public class UserStats
    {
        public int TotalUsers { get; set; }
        public int UsersCreatedThisMonth { get; set; }
        public int UsersCreatedThisYear { get; set; }
    }
}

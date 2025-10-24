using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GymManagmentSystem.Models;

namespace GymManagmentSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // POST: api/Auth/Login
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid email or password");
            }

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new LoginResponse
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth,
                    Occupation = user.Occupation
                },
                Roles = roles.ToList()
            });
        }

        // POST: api/Auth/Register
        [HttpPost("Register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Gender = request.Gender,
                Address = request.Address,
                DateOfBirth = request.DateOfBirth,
                Occupation = request.Occupation,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                // Add default role if it exists
                if (await _roleManager.RoleExistsAsync("Member"))
                {
                    await _userManager.AddToRoleAsync(user, "Member");
                }

                var token = await GenerateJwtToken(user);
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new RegisterResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Gender = user.Gender,
                        Address = user.Address,
                        DateOfBirth = user.DateOfBirth,
                        Occupation = user.Occupation
                    },
                    Roles = roles.ToList()
                });
            }

            return BadRequest(result.Errors);
        }

        // POST: api/Auth/RefreshToken
        [HttpPost("RefreshToken")]
        public async Task<ActionResult<LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid token");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("User not found");
            }

            var newToken = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new LoginResponse
            {
                Token = newToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    Address = user.Address,
                    DateOfBirth = user.DateOfBirth,
                    Occupation = user.Occupation
                },
                Roles = roles.ToList()
            });
        }

        // POST: api/Auth/Logout
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? "")
            };

            claims.AddRange(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(7);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Occupation { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
        public List<string> Roles { get; set; }
    }

    public class RegisterResponse
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
        public List<string> Roles { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Occupation { get; set; }
    }
}

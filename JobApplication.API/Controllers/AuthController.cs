using JobApplication.Application.DTOs.Auth;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Identity;
using JobApplication.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobApplication.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Register a new Recruiter or Candidate.
        /// Role must be "Recruiter" or "Candidate".
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto.Role != "Recruiter" && dto.Role != "Candidate")
                return BadRequest(new { message = "Role must be 'Recruiter' or 'Candidate'." });

            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                Role = dto.Role
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            // If the user is a Candidate, create the domain Candidate record
            if (dto.Role == "Candidate")
            {
                var candidate = new Candidate
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    AppUserId = user.Id,
                    CvUrl = string.Empty
                };
                _context.Candidates.Add(candidate);
                await _context.SaveChangesAsync();
            }

            var token = GenerateJwtToken(user);
            return Ok(new AuthResponseDto { Token = token, Email = user.Email!, Role = user.Role });
        }

        /// <summary>
        /// Login with email and password. Returns a JWT bearer token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
                return Unauthorized(new { message = "Invalid credentials." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid credentials." });

            var token = GenerateJwtToken(user);
            return Ok(new AuthResponseDto { Token = token, Email = user.Email!, Role = user.Role });
        }

        // ────────────────────────────────────────────────────────────────────────
        private string GenerateJwtToken(AppUser user)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryMinutes = int.TryParse(jwtSection["ExpiryMinutes"], out var mins) ? mins : 60;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(ClaimTypes.Role, user.Role),
                new("role", user.Role),
                new("name", user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

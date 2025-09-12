using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("generate-token")]
        public IActionResult GenerateToken([FromBody] TokenRequest request)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key"));
                
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, request.UserId ?? Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Name, request.Username ?? "TestUser"),
                        new Claim(ClaimTypes.Email, request.Email ?? "test@example.com"),
                        new Claim("role", "User")
                    }),
                    Expires = DateTime.UtcNow.AddHours(24),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Generated test token for user: {Username}", request.Username);

                return Ok(new TokenResponse
                {
                    Token = tokenString,
                    Expires = tokenDescriptor.Expires!.Value,
                    Type = "Bearer",
                    UserId = request.UserId ?? Guid.NewGuid().ToString(),
                    Username = request.Username ?? "TestUser"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("quick-token")]
        public IActionResult GenerateQuickToken()
        {
            try
            {
                var userId = Guid.NewGuid().ToString();
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key"));
                
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Name, "QuickTestUser"),
                        new Claim(ClaimTypes.Email, "quicktest@example.com"),
                        new Claim("role", "User")
                    }),
                    Expires = DateTime.UtcNow.AddHours(24),
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Generated quick test token for user: {UserId}", userId);

                return Ok(new TokenResponse
                {
                    Token = tokenString,
                    Expires = tokenDescriptor.Expires!.Value,
                    Type = "Bearer",
                    UserId = userId,
                    Username = "QuickTestUser"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating quick token");
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class TokenRequest
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }

    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public string Type { get; set; } = "Bearer";
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}
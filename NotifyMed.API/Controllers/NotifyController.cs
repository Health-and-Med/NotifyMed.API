using NotifyMed.Domain.Entities;
using NotifyMed.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NotifyMed.API.Controllers
{
    [ApiController]
    [Route("api/notify")]
    public class NotifyController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public NotifyController(IConfiguration configuration, IUserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel request)
        {
            try
            {
                var user = await _userService.AuthenticateAsync(request.Email, request.Password);
                if (user == null)
                    return Unauthorized();


                var token = GenerateJwtToken(request.Email);
                return Ok(new { token });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpGet("ping")]
        [Authorize]
        public IActionResult Ping()
        {
            return Ok(new { message = "Pong! Você está autenticado." });
        }

        [HttpPost("register")]
        [Authorize]
        public async Task<IActionResult> Register([FromBody] RegisterModel register)
        {
            try
            {
                await _userService.RegisterAsync(register.Username, register.Cpf, register.Crm, register.Email, register.Password, register.Role);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private string GenerateJwtToken(string email)
        {
            var jwtConfig = _configuration.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtConfig["Secret"]!);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtConfig["Issuer"],
                Audience = jwtConfig["Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}


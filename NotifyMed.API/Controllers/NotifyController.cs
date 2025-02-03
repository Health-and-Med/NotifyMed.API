using NotifyMed.Domain.Entities;
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

        public NotifyController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("ping")]
        [Authorize]
        public IActionResult Ping()
        {
            return Ok(new { message = "Pong! Você está autenticado." });
        }
    }
}


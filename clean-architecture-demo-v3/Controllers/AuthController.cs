using Application.Contracts;
using Application.Dto;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace clean_architecture_demo_v3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var userId = await _authService.RegisterAsync(registerDto);
            return Ok(new { UserId = userId, Message = "Registration successful." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);
            return Ok(response);
        }
    }
}
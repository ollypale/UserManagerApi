using Microsoft.AspNetCore.Mvc;
using UserManagerApi.Models.Requests;

namespace UserManagerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public AuthController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserRequest request)
        {
            var response = _jwtService.Authenticate(request);
            if (response == null)
                return Unauthorized("Неверный логин или пароль.");

            return Ok(response);
        }
    }
}

using Assiment5_MiniAmazonClone.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Assiment5_MiniAmazonClone.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Don't return password
            user.Password = null;

            return Ok(user);
        }
    }
}

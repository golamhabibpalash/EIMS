using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SMS.BLL.Contracts;
using SMS.Entities;
using SMS_App.Areas.API.Models;
using System;
using System.Threading.Tasks;

namespace SMS_App.Areas.API.Controllers
{
    [Area("API")]
    [Route("API/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DevicesController : ControllerBase
    {
        private readonly IMobileDeviceTokenManager _deviceTokenManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(
            IMobileDeviceTokenManager deviceTokenManager,
            UserManager<ApplicationUser> userManager,
            ILogger<DevicesController> logger)
        {
            _deviceTokenManager = deviceTokenManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] DeviceRegistrationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Token))
            {
                return BadRequest(new { message = "Device token is required." });
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid session." });
            }

            try
            {
                var saved = await _deviceTokenManager.RegisterAsync(userId, dto.Token.Trim(), dto.Platform);
                if (!saved)
                {
                    _logger.LogWarning("Device token registration failed for user {UserId}.", userId);
                    return StatusCode(503, new { message = "Device registration is temporarily unavailable." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Device token registration is unavailable for user {UserId}.", userId);
                return StatusCode(503, new { message = "Device registration is temporarily unavailable." });
            }

            return Ok(new { message = "Device registered." });
        }

        [HttpPost("unregister")]
        [AllowAnonymous]
        public async Task<IActionResult> Unregister([FromBody] DeviceRegistrationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Token))
            {
                return BadRequest(new { message = "Device token is required." });
            }

            try
            {
                await _deviceTokenManager.UnregisterAsync(dto.Token.Trim());
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Device token unregistration is unavailable.");
            }

            return Ok(new { message = "Device unregistered." });
        }
    }
}

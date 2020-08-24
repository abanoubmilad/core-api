using System.Security.Claims;
using core_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace core_api.Controllers
{
    [Authorize]
    [ApiController]
    public class Controller : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected ClaimsPrincipal GetClaims() => _httpContextAccessor.HttpContext.User;

        public Controller(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected IActionResult HandleServiceResponse(ServiceResponse response)
        {
            switch (response.ServiceError)
            {
                case ServiceError.None: return Ok(response);
                case ServiceError.Unauthorized: return Unauthorized(response);
                case ServiceError.Forbid: return Forbid();
                case ServiceError.BadRequest: return BadRequest(response);
                case ServiceError.NotFound: return NotFound(response);
                default: return StatusCode(424); // 424 Failed Dependency
            }
        }
    }
}
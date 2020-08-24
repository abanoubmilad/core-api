using System.Threading.Tasks;
using core_api.Data;
using core_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace core_api.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class TestsController : Controller
    {
        private readonly IAuthService _authRepo;
        private readonly IEmailService _emailService;

        public TestsController(IAuthService authRepo, IEmailService emailService, IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _authRepo = authRepo;
            _emailService = emailService;
        }


        [HttpPost("SendEmail")]
        public async Task<IActionResult> testEmail()
        {
            var message = new Message(new string[] { "abanoubcs@gmail.com" }, "Confirmation email link", "hello world");

            await _emailService.SendEmailAsync(message);

            return Ok();
        }


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var response = await _authRepo.ConfirmEmail(token, email);
            return HandleServiceResponse(response);
        }

    }
}
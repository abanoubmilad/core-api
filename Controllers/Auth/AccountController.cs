using System.Threading.Tasks;
using core_api.Data;
using core_api.Extentions;
using core_api.Models;
using core_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace core_api.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly IAuthService _authRepo;
        private readonly IEmailService _emailService;

        public AccountController(IAuthService authRepo, IEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _authRepo = authRepo;
            _emailService = emailService;
        }



        [AuthorizeRoles(Role.Admin, Role.Manager)]
        [HttpPost]
        public async Task<IActionResult> Invite(InviteUserRequest request)
        {
            var response = await _authRepo.Invite(request);

            if (response.Succeeded())
            {
                var url = Url.Action(nameof(ConfirmInvitation), ControllerContext.ActionDescriptor.ControllerName,
                    new { Token = response.Extra, Email = request.Email }
                    , Request.Scheme);

                var message = new Message(new string[] { response.Data.Email }, "Invitation email link", url);

                _emailService.SendEmail(message);
            }

            return HandleServiceResponse(response);
        }




        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var response = await _authRepo.Register(request);

            if (response.Succeeded())
            {
                var url = Url.Action(nameof(ConfirmEmail), ControllerContext.ActionDescriptor.ControllerName,
                    new { Token = response.Extra }, Request.Scheme);

                var message = new Message(new string[] { response.Data.Email }, "Confirmation email link", url);

                _emailService.SendEmail(message);
            }

            return HandleServiceResponse(response);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ConfirmInvitation(ResetPasswordRequest request)
        {
            var response = await _authRepo.ConfirmInvitation(request.Token,
                request.Email, request.NewPassword);
            return HandleServiceResponse(response);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _authRepo.Login(
                request.Email, request.Password
            );
            return HandleServiceResponse(response);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request)
        {
            var response = await _authRepo.ConfirmEmail(request.Token, request.Email);
            return HandleServiceResponse(response);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest request)
        {
            var response = await _authRepo.ForgetPassword(request.Email);

            if (response.Succeeded())
            {
                var url = Url.Action(nameof(ResetPassword), ControllerContext.ActionDescriptor.ControllerName,
                   new { Token = response.Extra, Email = request.Email }
                   , Request.Scheme);

                var message = new Message(new string[] { request.Email }, "Reset password email link", url);

                _emailService.SendEmail(message);
            }
            return HandleServiceResponse(response);
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var response = await _authRepo.ResetPassword(request.Token,
                request.Email, request.NewPassword);
            return HandleServiceResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var response = await _authRepo.ChangePassword(GetClaims().GetUserEmail(),
                request.CurrentPassword, request.NewPassword);
            return HandleServiceResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> FacebookSignIn(FacebookAuthRequest request)
        {
            var response = await _authRepo.FacebookSignIn(request.Token);
            return HandleServiceResponse(response);
        }

    }
}
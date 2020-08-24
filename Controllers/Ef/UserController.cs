using System.Threading.Tasks;
using core_api.Models;
using core_api.Services;
using core_api.Models.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace core_api.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUserService _service;

        public UsersController(IUserService service,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _service = service;
        }

        [AuthorizeRoles(Role.Admin, Role.Manager)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageQuery pageQery)
        {
            return HandleServiceResponse(await _service.FindAsync(pageQery));
        }

        [AuthorizeRoles(Role.Admin, Role.Manager)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            return HandleServiceResponse(await _service.FindByIdAsync(id));
        }



        [HttpPut]
        public async Task<IActionResult> Update(UpdateUserRequest request)
        {
            return HandleServiceResponse(await _service.UpdateAsync(GetClaims(), request));
        }
    }
}
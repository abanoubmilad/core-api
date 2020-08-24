using System.Threading.Tasks;
using core_api.Dtos;
using core_api.Models;
using core_api.Models.Pagination;
using core_api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace core_api.Controllers
{
    [Route("api/[controller]")]
    public class FirmsController : Controller
    {
        private readonly IFirmService _service;

        public FirmsController(IFirmService service,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageQuery pageQery)
        {
            return HandleServiceResponse(await _service.FindAsync(pageQery));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            return HandleServiceResponse(await _service.FindByIdAsync(id));
        }


        [AuthorizeRoles(Role.Admin, Role.Manager)]
        [HttpPost("{id}/GrantPermission")]
        public async Task<IActionResult> GrantPermission(int id, GrantPermissionRequest request)
        {
            return HandleServiceResponse(await _service.GrantPermissionAsync(GetClaims(), id, request));
        }




        [AuthorizeRoles(Role.Admin, Role.Manager)]
        [HttpPost]
        public async Task<IActionResult> Add(FirmDto request)
        {
            return HandleServiceResponse(await _service.AddAsync(GetClaims(), request));
        }

        [AuthorizeRoles(Role.Admin, Role.Manager)]
        [HttpPut]
        public async Task<IActionResult> Update(FirmDto request)
        {
            return HandleServiceResponse(await _service.UpdateAsync(GetClaims(), request));
        }

        [AuthorizeRoles(Role.Admin, Role.Manager)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return HandleServiceResponse(await _service.DeleteAsync(GetClaims(), id));
        }
    }
}
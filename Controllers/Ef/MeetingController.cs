using System.Threading.Tasks;
using core_api.Dtos;
using core_api.Models;
using core_api.Services;
using core_api.Models.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace core_api.Controllers
{
    [Route("api/Firms/{FirmId}/[controller]")]
    public class MeetingsController : Controller
    {
        private readonly IMeetingService _service;

        public MeetingsController(IMeetingService service,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromRoute] int firmId, [FromQuery] PageQuery pageQery)
        {
            return HandleServiceResponse(await _service.FindAsync(firmId, pageQery));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            return HandleServiceResponse(await _service.FindByIdAsync(id));
        }






        [AuthorizeRoles(Role.Admin, Role.Manager)]
        [HttpPost]
        public async Task<IActionResult> Add([FromRoute] int firmId, MeetingDto request)
        {
            return HandleServiceResponse(await _service.AddAsync(GetClaims(), firmId, request));
        }

        [AuthorizeRoles(Role.Admin, Role.Manager)]
        [HttpPut]
        public async Task<IActionResult> Update(MeetingDto request)
        {
            return HandleServiceResponse(await _service.UpdateAsync(GetClaims(), request));
        }

        [AuthorizeRoles(Role.Admin, Role.Manager)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            return HandleServiceResponse(await _service.DeleteAsync(GetClaims(), id));
        }
    }
}
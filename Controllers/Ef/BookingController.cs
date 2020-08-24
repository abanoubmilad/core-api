using System.Threading.Tasks;
using core_api.Dtos;
using core_api.Models;
using core_api.Services;
using core_api.Models.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace core_api.Controllers
{
    [Route("api/Meetings/{MeetingId}/[controller]")]
    public class BookingsController : Controller
    {
        private readonly IBookingService _service;

        public BookingsController(IBookingService service,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _service = service;
        }

        [AuthorizeRoles(Role.Admin, Role.Manager, Role.Organizer)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromRoute] int meetingId,
            [FromQuery] PageQuery pageQery)
        {
            return HandleServiceResponse(await _service.FindAsync(GetClaims(), meetingId, pageQery));
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {   // if no rule, check if this user has access to this booking
            return HandleServiceResponse(await _service.FindByIdAsync(GetClaims(), id));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromRoute] int meetingId, BookingDto request)
        {
            return HandleServiceResponse(await _service.AddAsync(GetClaims(), meetingId, request));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] BookingDto request)
        {   // if no rule, check if this user has access to this booking
            return HandleServiceResponse(await _service.UpdateAsync(GetClaims(), request));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // if no rule, check if this user has access to this booking
            // fail incase booking is already attended by user
            return HandleServiceResponse(await _service.DeleteAsync(GetClaims(), id));
        }
    }
}
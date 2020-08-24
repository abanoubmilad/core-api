using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using core_api.Data;
using core_api.Dtos;
using core_api.Extentions;
using core_api.Models;
using core_api.Models.Pagination;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace core_api.Services
{

    public interface IMeetingService
    {
        Task<ServiceResponse<PagedList<MeetingDto>>> FindAsync(int firmId, PageQuery pageQery);
        Task<ServiceResponse<MeetingDto>> FindByIdAsync(long id);

        Task<ServiceResponse<MeetingDto>> AddAsync(ClaimsPrincipal claims, int firmId, MeetingDto request);
        Task<ServiceResponse<MeetingDto>> UpdateAsync(ClaimsPrincipal claims, MeetingDto request);
        Task<ServiceResponse> DeleteAsync(ClaimsPrincipal claims, long id);
    }

    public class MeetingService : IMeetingService
    {
        private readonly EfRepository _repository;
        protected readonly IMapper _mapper;

        public MeetingService(IMapper mapper, EfRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<ServiceResponse<PagedList<MeetingDto>>> FindAsync(int firmId, PageQuery pageQery)
        {
            var response = new ServiceResponse<PagedList<MeetingDto>>();
            // todo make it single await for data and count
            var query = _repository.Query<Meeting>().Where(x => x.Firm.Id == firmId);

            var count = await _repository.CountAsync(query);
            var items = await _repository.ListPagiantionAsync(query, pageQery);
            response.Data = new PagedList<MeetingDto>(_mapper.Map<List<MeetingDto>>(items), count, pageQery);
            return response;
        }

        public async Task<ServiceResponse<MeetingDto>> FindByIdAsync(long id)
        {
            var response = new ServiceResponse<MeetingDto>();

            var item = await _repository.FindByIdAsync<Meeting>(id);

            if (item == null)
            {
                response.FailNotFound();
                return response;
            }

            response.Data = _mapper.Map<MeetingDto>(item);

            return response;
        }


        public async Task<ServiceResponse<MeetingDto>> AddAsync(ClaimsPrincipal claims, int firmId, MeetingDto request)
        {
            var response = new ServiceResponse<MeetingDto>();

            var found = await claims.GetFirmIfHasMinPermissionOf(_repository, firmId, Permission.Manage);

            if (found == null)
            {
                response.FailForbiden();
                return response;
            }

            var meeting = _mapper.Map<Meeting>(request);
            meeting.Firm = found;

            var result = await _repository.AddAsync(meeting);

            if (result == null)
            {
                response.FailOperation();
                return response;
            }
            response.Data = _mapper.Map<MeetingDto>(result);

            return response;
        }

        public async Task<ServiceResponse<MeetingDto>> UpdateAsync(ClaimsPrincipal claims, MeetingDto request)
        {
            var response = new ServiceResponse<MeetingDto>();

            var found = await claims.GetMeetingIfHasMinPermissionOf(_repository, request.Id, Permission.Manage);

            if (found == null)
            {
                response.FailForbiden();
                return response;
            }

            var result = await _repository.UpdateAsync(found.UpdateWith(request));
            if (result == 0)
            {
                response.FailOperation();
                return response;
            }
            response.Data = _mapper.Map<MeetingDto>(found);

            return response;
        }

        public async Task<ServiceResponse> DeleteAsync(ClaimsPrincipal claims, long id)
        {
            var response = new ServiceResponse<MeetingDto>();

            var found = await claims.GetMeetingIfHasMinPermissionOf(_repository, id, Permission.Manage);

            if (found == null)
            {
                response.FailForbiden();
                return response;
            }

            var result = await _repository.DeleteAsync(found);
            if (result == 0)
            {
                response.FailOperation();
            }

            return response;
        }
    }
}
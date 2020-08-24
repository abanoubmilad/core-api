using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using core_api.Data;
using core_api.Dtos;
using core_api.Extentions;
using core_api.Models;
using core_api.Models.Pagination;
using Microsoft.AspNetCore.Identity;

namespace core_api.Services
{

    public interface IFirmService
    {
        Task<ServiceResponse<PagedList<FirmDto>>> FindAsync(PageQuery pageQery);
        Task<ServiceResponse<FirmDto>> FindByIdAsync(int id);

        Task<ServiceResponse<FirmDto>> AddAsync(ClaimsPrincipal claims, FirmDto request);
        Task<ServiceResponse<FirmDto>> UpdateAsync(ClaimsPrincipal claims, FirmDto request);
        Task<ServiceResponse> DeleteAsync(ClaimsPrincipal claims, int id);
        Task<ServiceResponse<FirmUserDto>> GrantPermissionAsync(ClaimsPrincipal claims, int id, GrantPermissionRequest request);
    }

    public class FirmService : IFirmService
    {
        private readonly UserManager<User> _userManager;
        private readonly EfRepository _repository;
        protected readonly IMapper _mapper;

        public FirmService(IMapper mapper, EfRepository repository, UserManager<User> userManager)
        {
            _mapper = mapper;
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<PagedList<FirmDto>>> FindAsync(PageQuery pageQery)
        {
            var response = new ServiceResponse<PagedList<FirmDto>>();
            // todo make it single await for data and count
            var query = _repository.Query<Firm>();

            var count = await _repository.CountAsync(query);
            var items = await _repository.ListPagiantionAsync(query, pageQery);
            response.Data = new PagedList<FirmDto>(_mapper.Map<List<FirmDto>>(items), count, pageQery);
            return response;
        }

        public async Task<ServiceResponse<FirmDto>> FindByIdAsync(int id)
        {
            var response = new ServiceResponse<FirmDto>();

            var item = await _repository.FindByIdAsync<Firm>(id);

            if (item == null)
            {
                response.FailNotFound();
                return response;
            }

            response.Data = _mapper.Map<FirmDto>(item);

            return response;
        }




        public async Task<ServiceResponse<FirmDto>> AddAsync(ClaimsPrincipal claims, FirmDto request)
        {
            var response = new ServiceResponse<FirmDto>();

            var user = await _userManager.FindByIdAsync(claims.GetUserId());

            var firm = _mapper.Map<Firm>(request);
            var firmUser = new FirmUser()
            {
                Firm = firm,
                User = user,
                Permission = Permission.Manage
            };


            _repository.Add(firm);
            _repository.Add(firmUser);

            var result = await _repository.CommitAsnyc();
            if (result == 0)
            {
                response.FailOperation();
                return response;
            }
            response.Data = _mapper.Map<FirmDto>(firm);


            return response;
        }

        public async Task<ServiceResponse<FirmDto>> UpdateAsync(ClaimsPrincipal claims, FirmDto request)
        {
            var response = new ServiceResponse<FirmDto>();

            var found = await claims.GetFirmIfHasMinPermissionOf(_repository, request.Id, Permission.Manage);

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
            response.Data = _mapper.Map<FirmDto>(found);

            return response;
        }

        public async Task<ServiceResponse> DeleteAsync(ClaimsPrincipal claims, int id)
        {
            var response = new ServiceResponse<FirmDto>();

            var found = await claims.GetFirmIfHasMinPermissionOf(_repository, id, Permission.Manage);

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

        public async Task<ServiceResponse<FirmUserDto>> GrantPermissionAsync(ClaimsPrincipal claims, int id,
            GrantPermissionRequest request)
        {
            var response = new ServiceResponse<FirmUserDto>();

            var user = await _userManager.FindByIdAsync(claims.GetUserId());
            var found = await claims.GetFirmIfHasMinPermissionOf(_repository, id, Permission.Manage);

            if (found == null)
            {
                response.FailForbiden();
                return response;
            }
            var firmUser = new FirmUser()
            {
                User = user,
                Firm = found
            };
            var result = await _repository.AddAsync(firmUser);
            if (result == null)
            {
                response.FailOperation();
            }

            response.Data = _mapper.Map<FirmUserDto>(result);

            return response;
        }
    }
}
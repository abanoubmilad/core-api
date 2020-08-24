using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using core_api.Data;
using core_api.Extentions;
using core_api.Models;
using core_api.Models.Pagination;
using Microsoft.AspNetCore.Identity;

namespace core_api.Services
{
    public interface IUserService
    {
        Task<ServiceResponse<PagedList<UserDto>>> FindAsync(PageQuery pageQery);
        Task<ServiceResponse<UserDto>> FindByIdAsync(string id);
        Task<ServiceResponse<UserDto>> UpdateAsync(ClaimsPrincipal claims, UpdateUserRequest request);
    }

    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly EfRepository _repository;
        protected readonly IMapper _mapper;

        public UserService(IMapper mapper, EfRepository repository, UserManager<User> userManager)
        {
            _mapper = mapper;
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<PagedList<UserDto>>> FindAsync(PageQuery pageQery)
        {
            var response = new ServiceResponse<PagedList<UserDto>>();
            // todo make it single await for data and count
            var query = _repository.Query<User>();

            var count = await _repository.CountAsync(query);
            var items = await _repository.ListPagiantionAsync(query, pageQery);
            response.Data = new PagedList<UserDto>(_mapper.Map<List<UserDto>>(items), count, pageQery);
            return response;
        }

        public async Task<ServiceResponse<UserDto>> FindByIdAsync(string id)
        {
            var response = new ServiceResponse<UserDto>();

            var item = await _repository.FindByIdAsync<User>(id);

            if (item == null)
            {
                response.FailNotFound();
                return response;
            }

            response.Data = _mapper.Map<UserDto>(item);

            return response;
        }


        public async Task<ServiceResponse<UserDto>> UpdateAsync(ClaimsPrincipal claims, UpdateUserRequest request)
        {
            var response = new ServiceResponse<UserDto>();

            // role configuration
            if (!claims.HasAccessToUser(request.Id))
            {
                response.FailForbiden();
                return response;
            }

            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                response.FailNotFound();
                return response;
            }

            var result = await _userManager.UpdateAsync(user.UpdateWith(request));
            if (!result.Succeeded)
            {
                response.FailOperation();
                return response;
            }

            response.Data = _mapper.Map<UserDto>(result);

            return response;
        }

    }
}
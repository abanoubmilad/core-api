using System;
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


    public interface IBookingService
    {
        Task<ServiceResponse<PagedList<BookingDto>>> FindAsync(ClaimsPrincipal claims,
            long meetingId, PageQuery pageQery);

        Task<ServiceResponse<BookingDto>> FindByIdAsync(ClaimsPrincipal claims, long id);

        Task<ServiceResponse<BookingDto>> AddAsync(ClaimsPrincipal claims,
            long meetingId, BookingDto request);

        Task<ServiceResponse<BookingDto>> UpdateAsync(ClaimsPrincipal claims, BookingDto request);

        Task<ServiceResponse> DeleteAsync(ClaimsPrincipal claims, long id);
    }

    public class BookingService : IBookingService
    {
        private readonly UserManager<User> _userManager;
        private readonly EfRepository _repository;
        protected readonly IMapper _mapper;

        public BookingService(IMapper mapper, EfRepository repository, UserManager<User> userManager)
        {
            _mapper = mapper;
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<ServiceResponse<PagedList<BookingDto>>> FindAsync(ClaimsPrincipal claims,
           long meetingId, PageQuery pageQery)

        {
            var response = new ServiceResponse<PagedList<BookingDto>>();
            // todo make it single await for data and count
            var query = _repository.Query<Booking>().Where(x => x.Meeting.Id == meetingId);

            var count = await _repository.CountAsync(query);
            var items = await _repository.ListPagiantionAsync(query, pageQery);
            response.Data = new PagedList<BookingDto>(_mapper.Map<List<BookingDto>>(items), count, pageQery);
            return response;
        }

        public async Task<ServiceResponse<BookingDto>> FindByIdAsync(ClaimsPrincipal claims, long id)
        {
            var response = new ServiceResponse<BookingDto>();

            var found = await claims.GetBookingIfHasMinPermissionOf(_repository, id, Permission.Organize);

            if (found == null)
            {
                response.FailNotFound();
                return response;
            }

            response.Data = _mapper.Map<BookingDto>(found);

            return response;
        }

        public async Task<ServiceResponse<BookingDto>> AddAsync(ClaimsPrincipal claims,
            long meetingId, BookingDto request)
        {
            var response = new ServiceResponse<BookingDto>();

            var user = await _userManager.FindByIdAsync(claims.GetUserId());
            var meeting = await _repository.FindByIdAsync<Meeting>(meetingId);


            if (meeting == null)
            {
                response.FailNotFound();
                return response;
            }

            // todo add validations of booking

            if (meeting.StartAt < DateTime.UtcNow)
            {
                response.FailOperation("Meeting already passed.");
                return response;
            }

            if (meeting.BookedCount >= meeting.MaxAllowedBookings)
            {
                response.FailOperation("Meeting already full.");
                return response;
            }

            var lastBooking = await _repository.Query<Booking>().Where(x => x.BookedBy.Id == user.Id &&
                x.Meeting.Id == meeting.Id).MaxAsync(x => x.RequestedAt);

            if (lastBooking.AddHours(meeting.MinIntervalBetweenMeetingsInHours) < meeting.StartAt)
            {
                response.FailOperation("You have recently requested a booking.");
                return response;
            }

            var booking = _mapper.Map<Booking>(request);
            booking.Meeting = meeting;
            booking.BookedBy = user;

            meeting.BookedCount += 1;

            _repository.Add(booking);
            _repository.Update(meeting);

            var result = await _repository.CommitAsnyc();
            if (result == 0)
            {
                response.FailOperation();
                return response;
            }

            response.Data = _mapper.Map<BookingDto>(booking);

            return response;
        }

        public async Task<ServiceResponse<BookingDto>> UpdateAsync(ClaimsPrincipal claims, BookingDto request)
        {
            var response = new ServiceResponse<BookingDto>();

            var found = await claims.GetBookingIfHasMinPermissionOf(_repository, request.Id, Permission.Manage);

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
            response.Data = _mapper.Map<BookingDto>(found);

            return response;
        }

        public async Task<ServiceResponse> DeleteAsync(ClaimsPrincipal claims, long id)
        {
            var response = new ServiceResponse<BookingDto>();

            var found = await claims.GetBookingIfHasMinPermissionOf(_repository, id, Permission.Manage);

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
using AutoMapper;
using core_api.Dtos;
using core_api.Models;

namespace core_api
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User
            CreateMap<User, UserDto>();
            CreateMap<User, UserProfileDto>();
            CreateMap<RegisterRequest, User>()
                .ForMember(u => u.UserName, opt => opt.MapFrom(x => x.Email));

            // Booking
            CreateMap<Booking, BookingDto>();
            CreateMap<BookingDto, Booking>();

            // Meeting
            CreateMap<Meeting, MeetingDto>();
            CreateMap<MeetingDto, Meeting>();

            // Firm
            CreateMap<Firm, FirmDto>();
            CreateMap<FirmDto, Firm>();

            // FirmUser
            CreateMap<FirmUser, FirmUserDto>();


        }
    }
}
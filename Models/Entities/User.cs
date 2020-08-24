using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace core_api.Models
{
    public class User : IdentityUser
    {
        public string Role { get; set; }

        public long FacebookId { get; set; }
        public string PhotoUrl { get; set; }
        public string FullName { get; set; }
        public string NationalId { get; set; }

        public bool Verified { get; set; }

        // map
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<FirmUser> FirmUsers { get; set; }

        internal User UpdateWith(UpdateUserRequest request)
        {
            // once a user is verified, he can not change his name or national id
            if (!Verified)
            {
                FullName = request.FullName;
                NationalId = request.NationalId;
            }
            PhoneNumber = request.PhoneNumber;
            return this;

        }
    }
}
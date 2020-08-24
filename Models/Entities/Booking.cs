using System;
using core_api.Dtos;

namespace core_api.Models
{
    public class Booking
    {
        public long Id { get; set; }

        public DateTime RequestedAt { get; set; }

        public bool Approved { get; set; }
        public DateTime ApprovedAt { get; set; }

        public bool Attended { get; set; }
        public DateTime AttendedAt { get; set; }

        public string Note { get; set; }

        // map
        public User BookedBy { get; set; }
        public Meeting Meeting { get; set; }

        internal Booking UpdateWith(BookingDto request)
        {
            Note = request.Note;

            return this;
        }
    }
}
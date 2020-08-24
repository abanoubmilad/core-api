using System;
using System.Collections.Generic;
using core_api.Dtos;

namespace core_api.Models
{
    public class Meeting
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public DateTime StartAt { get; set; }
        public int DurationInMins { get; set; }

        public bool Active { get; set; }

        public int BookedCount { get; set; }

        public int MaxAllowedBookings { get; set; }

        public int MinIntervalBetweenMeetingsInHours { get; set; }

        // map
        public Firm Firm { get; set; }

        public ICollection<Booking> Bookings { get; set; }

        internal Meeting UpdateWith(MeetingDto request)
        {
            Name = request.Name;
            return this;
        }

    }
}
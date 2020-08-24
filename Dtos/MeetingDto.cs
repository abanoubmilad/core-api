using System;
using System.ComponentModel.DataAnnotations;

namespace core_api.Dtos
{
    public class MeetingDto
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartAt { get; set; }

        [Range(10, 720)]
        public int DurationInMins { get; set; } = 120;

        public bool Active { get; set; } = true;

        public int BookedCount { get; set; }

        [Range(1, int.MaxValue)]
        public int MaxAllowedBookings { get; set; } = 50;
        [Range(1, int.MaxValue)]
        public int MinIntervalBetweenMeetingsInHours { get; set; } = 24 * 7;

    }
}

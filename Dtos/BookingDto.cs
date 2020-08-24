using System;
namespace core_api.Dtos
{
    public class BookingDto
    {
        public long Id { get; set; }

        public DateTime RequestedAt { get; set; }

        public bool Approved { get; set; }
        public DateTime ApprovedAt { get; set; }

        public bool Attended { get; set; }
        public DateTime AttendedAt { get; set; }

        public string Note { get; set; }
    }
}

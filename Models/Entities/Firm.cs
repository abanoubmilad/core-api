using System.Collections.Generic;
using core_api.Dtos;

namespace core_api.Models
{
    public class Firm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }

        // map
        public ICollection<Meeting> Meetings { get; set; }
        public ICollection<FirmUser> FirmUsers { get; set; }


        public Firm UpdateWith(FirmDto request)
        {
            Name = request.Name;
            Address = request.Address;

            return this;
        }
    }

}
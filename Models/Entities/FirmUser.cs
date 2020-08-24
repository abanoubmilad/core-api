using System.Collections.Generic;

namespace core_api.Models
{
    public class FirmUser
    {
        public Permission Permission { get; set; }
        // map
        public Firm Firm { get; set; }
        public int FirmId { get; set; }

        public User User { get; set; }
        public string UserId { get; set; }
    }

}
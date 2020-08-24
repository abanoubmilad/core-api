using System;
using System.ComponentModel.DataAnnotations;
using core_api.Models;

namespace core_api.Dtos
{

    public class FirmUserDto
    {
        public FirmDto Firm { get; set; }
        public UserDto User { get; set; }
    }


    public class GrantPermissionRequest
    {
        public Permission Permission { get; set; }
        public string UserId { get; set; }
    }
}

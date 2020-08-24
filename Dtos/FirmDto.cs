using System;
using System.ComponentModel.DataAnnotations;
using core_api.Models;

namespace core_api.Dtos
{

    public class FirmDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
    }

}

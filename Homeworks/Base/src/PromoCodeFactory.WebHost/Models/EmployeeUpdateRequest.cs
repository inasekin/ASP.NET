using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PromoCodeFactory.WebHost.Models
{
    public class EmployeeUpdateRequest
    {
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        [Required]
        public List<string> RoleIdList { get; set; }
    }
}
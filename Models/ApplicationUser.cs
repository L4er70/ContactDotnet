using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ContactBook.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
    }
}
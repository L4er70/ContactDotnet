using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContactBook.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        public ICollection<Phone> Phones { get; set; } = new List<Phone>();

        public ICollection<Address> Addresses { get; set; } = new List<Address>();

        public ICollection<Email> Emails { get; set; } = new List<Email>();
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContactBook.Models
{
    public class Contact
    {
        public Contact()
        {
            Phones = new List<Phone>();
            Emails = new List<Email>();
            Addresses = new List<Address>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        public string LastName { get; set; } = string.Empty;

        public ICollection<Phone> Phones { get; set; }

        public ICollection<Address> Addresses { get; set; }

        public ICollection<Email> Emails { get; set; }
    }
}

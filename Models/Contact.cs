using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ContactBook.Models{
    public class Contact{
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public ICollection<Phone> Phones { get; set; }
        public ICollection<Address> Addresses { get; set; }
        public ICollection<Email> Emails { get; set; }
    }
}
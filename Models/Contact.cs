using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ContactBook.Models{
    public class Contact{
        public int Id { get; set; }
        [Required]
        
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        public required ICollection<Phone> Phones { get; set; }
        public required ICollection<Address> Addresses { get; set; }
        public required ICollection<Email> Emails { get; set; }
    }
}
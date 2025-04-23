using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ContactBook.Models{
    public class Contact{
        public int Id { get; set; }
        [Required]
        
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }
        public required ICollection<Phone> Phones { get; set; }=new List<Phone>();
        public required ICollection<Address> Addresses { get; set; }=new List<Address>();
        public required ICollection<Email> Emails { get; set; }=    new List<Email>();
    }
}
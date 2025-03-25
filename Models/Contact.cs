using System.Collections.Generic;
namespace ContactBook.Models{
    public class Contact{
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<Phone> Phones { get; set; }
        public ICollection<Address> Addresses { get; set; }
        public ICollection<Email> Emails { get; set; }
    }
}
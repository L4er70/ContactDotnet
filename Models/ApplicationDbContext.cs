using Microsoft.EntityFrameworkCore;
namespace ContactBook.Models{
    public class ApplicationDbContext : DbContext{
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Email> Emails { get; set; }
    }
}
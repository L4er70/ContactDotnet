namespace ContactBook.Models{
    public class Phone{
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public int ContactId { get; set; }
        public Contact Contact { get; set; } = null!;
    }
}
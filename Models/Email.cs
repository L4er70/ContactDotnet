namespace ContactBook.Models{
    public class Email{
        public int Id { get; set; }
        public required string EmailAddress { get; set; }
        public int ContactId { get; set; }
        public Contact? Contact { get; set; }
    }
}
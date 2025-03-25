namespace ContactBook.Models{
    public class Address{
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public int ContactId { get; set; }
        public Contact Contact { get; set; }
    }
}
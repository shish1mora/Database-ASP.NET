namespace Publishing_house.Models
{
    public class BookingEmployee
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public Booking? Booking { get; set; }
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
    }
}

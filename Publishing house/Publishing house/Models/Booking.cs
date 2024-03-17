using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Publishing_house.Models
{
    public class Booking
    {
        [DisplayName("Номер заказа")]
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите дату приёма")]
        [DataType(DataType.Date)]
        [DisplayName("Дата приёма")]
        public DateTime Start { get; set; }

        [Required(ErrorMessage = "Введите дату выполнения")]
        [DataType(DataType.Date)]
        [DisplayName("Дата выполнения")]
        public DateTime End { get; set; }
        [Required(ErrorMessage = "Введите статус заказа")]
        [DataType(DataType.Text)]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Длина строки не входит в заданный диапазон: [5, 20]")]
        [DisplayName("Статус заказа")]
        [RegularExpression("^[А-Яа-я ]+$", ErrorMessage = "Разрешается ввести только русские буквы")]
        public string Status { get; set; }
        [Required(ErrorMessage = "Введите стоимость заказа")]
        [DisplayName("Стоимость заказа")]
        [DataType(DataType.Currency)] // Игнорируется мой ErrorMessage
        [Range(typeof(double), "0,1", "100000000", ErrorMessage = "Стоимость должна входить в диапазон: [0.1, 100000000]")]
        public double Cost { get; set; }

        public int PrintingHouseId { get; set; }
        public PrintingHouse? PrintingHouse { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public virtual ICollection<BookingEmployee> Bookings { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public Booking()
        {
            Bookings = new List<BookingEmployee>();
            Products = new List<Product>();
        }
    }
}


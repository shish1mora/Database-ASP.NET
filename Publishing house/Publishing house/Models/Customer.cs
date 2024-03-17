using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Publishing_house.Models
{
    public class Customer
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите наименование заказчика")]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Длина строки не входит в заданный диапазон: [1, 50]")]
        [DisplayName("Наименование заказчика")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Введите номер телефона")]
        [DataType(DataType.PhoneNumber)]
        [DisplayName("Номер телефона")]
        [RegularExpression(@"\+7-\d{3}-\d{3}-\d{2}-\d{2}", ErrorMessage = "Неверный номер телефона.Паттерн: +7 -###-###-##-##")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Введите электронную почту")]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Электронная почта")]
        [MaxLength(50)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Введена неверная электронная почта")]
        public string Email { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public Customer()
        {
            Bookings = new List<Booking>();
        }
    }

}

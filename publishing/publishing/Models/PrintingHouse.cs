using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace publishing.Models
{
    public class PrintingHouse
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название типографии")]
        [DataType(DataType.Text)]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Длина строки не входит в заданный диапазон:[1,20]")]
        [DisplayName("Название типографии")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Введите номер телефона")]
        [DataType(DataType.PhoneNumber)]
        [DisplayName("Номер телефона")]
        [RegularExpression(@"\+7-\d{3}-\d{3}-\d{2}-\d{2}", ErrorMessage = "Неверный номер телефона.Паттерн: +7-###-###-##-##")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Введите электронную почту")]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Электронная почта")]
        [MaxLength(50)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage = "Введена неверная электронная почта")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Введите субъект РФ")]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "Длина строки не входит в заданный диапазон:[10,50]")]
        [DisplayName("Субъект РФ")]
        [RegularExpression("^[А-Яа-я ]+$", ErrorMessage = "Разрешается ввести только русские буквы")]
        public string State { get; set; }


        [Required(ErrorMessage = "Введите город")]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Длина строки не входит в заданный диапазон:[3,50]")]
        [DisplayName("Город")]
        [RegularExpression(@"^[А-Яа-я]*(?:[\s-][А-Яа-я]*)*$", ErrorMessage = "Неверный ввод названия города")]
        public string City { get; set; }

        [Required(ErrorMessage = "Введите улицу")]
        [DataType(DataType.Text)]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Длина строки не входит в заданный диапазон:[3,50]")]
        [DisplayName("Улица")]
        [RegularExpression("^[А-Яа-я ]+$", ErrorMessage = "Разрешается ввести только русские буквы")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Введите номер дома")]
        [DataType(DataType.Text)]
        [StringLength(7, MinimumLength = 1, ErrorMessage = "Длина строки не входит в заданный диапазон:[1,7]")]
        [DisplayName("Номер дома")]
        [RegularExpression(@"^[1-9]\d*(?: ?(?:[А-Га-г]|[/] ?\d+))?$", ErrorMessage = "Неправильный ввод номера дома. Пример 23A, 23, 23/2")]
        public string House { get; set; }

        [Required(ErrorMessage = "Введите дополнительную инфомацию")]
        [DisplayName("Дополнительная информация")]
        public string Description { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }

        public PrintingHouse() 
        { 
            Bookings = new List<Booking>(); 
        }

    }
}

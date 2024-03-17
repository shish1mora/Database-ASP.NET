using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Publishing_house.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите имя сотрудника")]
        [DataType(DataType.Text)]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Длина строки не входит в заданный диапазон: [1, 20]")]
        [DisplayName("Имя сотрудника")]
        [RegularExpression("^[А-Яа-я ]+$", ErrorMessage = "Разрешается ввести только русские буквы")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Введите фамилию сотрудника")]
        [DataType(DataType.Text)]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Длина строки не входит в  заданный диапазон: [1, 20]")]
        [DisplayName("Фамилия сотрудника")]
        [RegularExpression("^[А-Яа-я ]+$", ErrorMessage = "Разрешается ввести только русские буквы")]
        public string Surname { get; set; }
        [DataType(DataType.Text)]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "Длина строки не входит в заданный диапазон: [1, 20]")]
        [DisplayName("Отчество сотрудника")]
        [RegularExpression("^[А-Яа-я ]+$", ErrorMessage = "Разрешается ввести только русские буквы")]
        public string? Middlename { get; set; }
        [Required(ErrorMessage = "Введите должность сотрудника")]
        [DataType(DataType.Text)]
        [StringLength(30, MinimumLength = 5, ErrorMessage = "Длина строки не входит в заданный диапазон: [5, 30]")]
        [DisplayName("Должность сотрудника")]
        [RegularExpression("^[А-Яа-я ]+$", ErrorMessage = "Разрешается ввести только русские буквы")]
        public string Type { get; set; }
        [Required(ErrorMessage = "Введите номер телефона")]
        [DataType(DataType.PhoneNumber)]
        [DisplayName("Номер телефона")]
        [RegularExpression(@"\+7-\d{3}-\d{3}-\d{2}-\d{2}", ErrorMessage = "Неверный номер телефона.Паттерн: +7 -###-###-##-##")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Введите электронную почту")]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Электронная почта")]
        [MaxLength(50)]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessage =
       "Введена неверная электронная почта")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Введите дату рождения")]
        [DataType(DataType.Date)]
        [DisplayName("Дата рождения")]
        public DateTime Birthday { get; set; }
        [Required(ErrorMessage = "Выберите фото сотрудника")]
        [DataType(DataType.ImageUrl)]
        [DisplayName("Фото сотрудника")]
        public string Visual { get; set; }
        public virtual ICollection<BookingEmployee> BookingEmployees { get; set; }
        public Employee()
        {
            BookingEmployees = new List<BookingEmployee>();
        }
    }

}

using System.ComponentModel.DataAnnotations;

namespace WpfApp1.Models.Validation
{
    public class SupplierValidationModel
    {
        [Required(ErrorMessage = "Имя обязательно.")]
        public string name { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна.")]
        public string last_name { get; set; }

        [Required(ErrorMessage = "Отчество обязательно.")]
        public string surname { get; set; }

        [Required(ErrorMessage = "Телефон обязателен.")]
        [Phone(ErrorMessage = "Некорректный формат телефона.")]
        public string phone { get; set; }

        [Required(ErrorMessage = "Email обязателен.")]
        [EmailAddress(ErrorMessage = "Некорректный email.")]
        public string email { get; set; }
    }
}
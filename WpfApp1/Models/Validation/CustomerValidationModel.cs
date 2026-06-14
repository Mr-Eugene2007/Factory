using System.ComponentModel.DataAnnotations;

namespace WpfApp1.Models.Validation
{
    /// <summary>
    /// Модель данных для валидации клиента.
    /// Используется совместно с CustomerValidator и атрибутами DataAnnotations.
    /// </summary>
    public class CustomerValidationModel
    {
        [Required(ErrorMessage = "Имя обязательно.")]
        public string name { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна.")]
        public string last_name { get; set; }

        [Required(ErrorMessage = "Отчество обязательно.")]
        public string surname { get; set; }

        [Required(ErrorMessage = "Телефон обязателен.")]
        public string phone { get; set; }

        [Required(ErrorMessage = "Email обязателен.")]
        public string email { get; set; }
    }
}
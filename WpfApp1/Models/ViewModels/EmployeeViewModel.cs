using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using WpfApp1.Pages;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// ViewModel для представления данных сотрудника.
    /// Используется для привязки данных в интерфейсе и передачи информации в сервисы.
    /// </summary>
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        private int _id;
        private string _lastName;
        private string _name;
        private string _surname;
        private string _phone;
        private string _email;
        private string _role;
        private bool _isSupplier;
        private bool _isCustomer;
        private bool _hasSystemAccess;
        private bool _isActive;
        private DateTime _createdDate;
        private string _login;
        private int? _authoId;
        private object _originalEntity;
        private string _status;

        public string Password { get; set; }

        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }
        public string LastName { get => _lastName; set { _lastName = value; OnPropertyChanged(); } }
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public string Surname { get => _surname; set { _surname = value; OnPropertyChanged(); } }
        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string Role { get => _role; set { _role = value; OnPropertyChanged(); } }
        public bool IsSupplier { get => _isSupplier; set { _isSupplier = value; OnPropertyChanged(); } }
        public bool IsCustomer { get => _isCustomer; set { _isCustomer = value; OnPropertyChanged(); } }
        public bool HasSystemAccess { get => _hasSystemAccess; set { _hasSystemAccess = value; OnPropertyChanged(); } }
        public bool IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged(); } }
        public DateTime CreatedDate { get => _createdDate; set { _createdDate = value; OnPropertyChanged(); } }
        public string Login { get => _login; set { _login = value; OnPropertyChanged(); } }
        public int? AuthoId { get => _authoId; set { _authoId = value; OnPropertyChanged(); } }
        public object OriginalEntity { get => _originalEntity; set { _originalEntity = value; OnPropertyChanged(); } }
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        public string FullName => $"{LastName} {Name} {Surname}".Trim();

        /// <summary>
        /// Создаёт новый объект сотрудника с заполненными значениями по умолчанию.
        /// </summary>
        public static EmployeeViewModel CreateNew(string defaultRole)
        {
            return new EmployeeViewModel
            {
                Role = defaultRole == "Клиент" ? "Клиент" : "Поставщик",
                IsCustomer = defaultRole == "Клиент",
                IsSupplier = defaultRole != "Клиент",
                CreatedDate = DateTime.Now,
                IsActive = true,
                Status = "Активен"
            };
        }

        /// <summary>
        /// Создаёт ViewModel на основе существующей записи.
        /// Используется при редактировании сотрудника.
        /// </summary>
        public static EmployeeViewModel FromExisting(AuthoAdmin.EmployeeViewModel src, object original)
        {
            return new EmployeeViewModel
            {
                Id = src.Id,
                LastName = src.LastName,
                Name = src.Name,
                Surname = src.Surname,
                Phone = src.Phone,
                Email = src.Email,
                Role = src.Role,
                IsSupplier = src.Role == "Поставщик",
                IsCustomer = src.Role == "Клиент",
                HasSystemAccess = src.HasSystemAccess,
                IsActive = src.IsActive,
                CreatedDate = src.CreatedDate,
                Login = src.Login,
                AuthoId = src.AuthoId,
                OriginalEntity = original,
                Status = src.Status
            };
        }

        /// <summary>
        /// Событие, уведомляющее интерфейс об изменении свойств.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Вызывает событие PropertyChanged для обновления UI.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Models;
using WpfApp1.Pages;
using WpfApp1.Services;

namespace WpfApp1.Pages
{
    public partial class EditEmployeePage : Page, INotifyPropertyChanged
    {
        private BeverageFactoryEntities _context;
        private EmployeeViewModel _employeeVM;
        private AuthoAdmin.EmployeeViewModel _selectedEmployee;
        private Autho _currentUser;
        private string _mode;
        private object _originalEntity;

        // Статическое событие для уведомления об обновлении данных
        public static event Action DataUpdated;

        public event PropertyChangedEventHandler PropertyChanged;

        public EmployeeViewModel EmployeeVM
        {
            get => _employeeVM;
            set
            {
                _employeeVM = value;
                OnPropertyChanged();
            }
        }

        public EditEmployeePage(Autho currentUser, string defaultRole = null)
        {
            InitializeComponent();
            _context = new BeverageFactoryEntities();
            _currentUser = currentUser;
            _mode = "add";
            InitializeNewEmployee(defaultRole);
            DataContext = EmployeeVM;
        }

        public EditEmployeePage(AuthoAdmin.EmployeeViewModel employee, Autho currentUser)
        {
            InitializeComponent();
            _context = new BeverageFactoryEntities();
            _selectedEmployee = employee;
            _currentUser = currentUser;
            _mode = "edit";
            LoadEmployeeForEdit();
            DataContext = EmployeeVM;
        }

        private void InitializeNewEmployee(string defaultRole)
        {
            EmployeeVM = new EmployeeViewModel
            {
                Id = 0,
                LastName = "",
                Name = "",
                Surname = "",
                Phone = "",
                Email = "",
                Role = defaultRole == "Клиент" ? "Клиент" : "Поставщик",
                IsSupplier = defaultRole != "Клиент",
                IsCustomer = defaultRole == "Клиент",
                HasSystemAccess = false,
                IsActive = true,
                CreatedDate = DateTime.Now,
                Login = "",
                Status = "Активен"
            };

            _originalEntity = null;
            btnDelete.Visibility = Visibility.Collapsed;
        }

        private void LoadEmployeeForEdit()
        {
            if (_selectedEmployee == null) return;

            // Получаем оригинальную сущность из базы данных
            if (_selectedEmployee.Role == "Поставщик")
            {
                _originalEntity = _context.Suppliers.FirstOrDefault(s => s.id == _selectedEmployee.Id);
            }
            else if (_selectedEmployee.Role == "Клиент")
            {
                _originalEntity = _context.Customers.FirstOrDefault(c => c.id == _selectedEmployee.Id);
            }

            EmployeeVM = new EmployeeViewModel
            {
                Id = _selectedEmployee.Id,
                LastName = _selectedEmployee.LastName,
                Name = _selectedEmployee.Name,
                Surname = _selectedEmployee.Surname,
                Phone = _selectedEmployee.Phone,
                Email = _selectedEmployee.Email,
                Role = _selectedEmployee.Role,
                IsSupplier = _selectedEmployee.Role == "Поставщик",
                IsCustomer = _selectedEmployee.Role == "Клиент",
                HasSystemAccess = _selectedEmployee.HasSystemAccess,
                IsActive = _selectedEmployee.IsActive,
                CreatedDate = _selectedEmployee.CreatedDate,
                Login = _selectedEmployee.Login,
                AuthoId = _selectedEmployee.AuthoId,
                OriginalEntity = _originalEntity,
                Status = _selectedEmployee.Status
            };

            btnDelete.Visibility = Visibility.Visible;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm())
                    return;

                if (_mode == "edit")
                {
                    UpdateEmployee();
                }
                else
                {
                    CreateEmployee();
                }

                MessageBox.Show("Данные сотрудника успешно сохранены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Вызываем событие обновления данных
                NotifyDataUpdated();

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(EmployeeVM.LastName))
            {
                MessageBox.Show("Введите фамилию", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmployeeVM.Name))
            {
                MessageBox.Show("Введите имя", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmployeeVM.Phone))
            {
                MessageBox.Show("Введите телефон", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmployeeVM.Email))
            {
                MessageBox.Show("Введите email", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            // Проверка email
            if (!IsValidEmail(EmployeeVM.Email))
            {
                MessageBox.Show("Введите корректный email", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            // Проверка данных авторизации
            if (EmployeeVM.HasSystemAccess)
            {
                if (string.IsNullOrWhiteSpace(EmployeeVM.Login))
                {
                    MessageBox.Show("Введите логин для доступа к системе", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtLogin.Focus();
                    return false;
                }

                if (_mode == "add" && (string.IsNullOrWhiteSpace(txtPassword.Password) ||
                    string.IsNullOrWhiteSpace(txtPasswordConfirm.Password)))
                {
                    MessageBox.Show("Введите пароль и подтверждение пароля", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return false;
                }

                if (_mode == "add" && txtPassword.Password != txtPasswordConfirm.Password)
                {
                    MessageBox.Show("Пароли не совпадают", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return false;
                }

                // Проверка уникальности логина
                if (_context.Authoes.Any(a => a.login == EmployeeVM.Login &&
                    (_mode == "add" || (_mode == "edit" && EmployeeVM.AuthoId != a.id))))
                {
                    MessageBox.Show("Этот логин уже используется", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtLogin.Focus();
                    return false;
                }
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void UpdateEmployee()
        {
            if (EmployeeVM.Role == "Поставщик" && _originalEntity is Supplier supplier)
            {
                // Обновляем свойства сущности
                supplier.last_name = EmployeeVM.LastName;
                supplier.name = EmployeeVM.Name;
                supplier.surname = EmployeeVM.Surname;
                supplier.phone = EmployeeVM.Phone;
                supplier.email = EmployeeVM.Email;

                // Обновляем данные авторизации
                var updatedAuthoId = UpdateAuthoData(supplier.autho_id);
                supplier.autho_id = updatedAuthoId;

                // Сохраняем изменения
                _context.SaveChanges();
            }
            else if (EmployeeVM.Role == "Клиент" && _originalEntity is Customer customer)
            {
                // Обновляем свойства сущности
                customer.last_name = EmployeeVM.LastName;
                customer.name = EmployeeVM.Name;
                customer.surname = EmployeeVM.Surname;
                customer.phone = EmployeeVM.Phone;
                customer.email = EmployeeVM.Email;

                // Обновляем данные авторизации
                var updatedAuthoId = UpdateAuthoData(customer.autho_id);
                customer.autho_id = updatedAuthoId;

                // Сохраняем изменения
                _context.SaveChanges();
            }
        }

        private void CreateEmployee()
        {
            if (EmployeeVM.IsSupplier)
            {
                var newSupplier = new Supplier
                {
                    last_name = EmployeeVM.LastName,
                    name = EmployeeVM.Name,
                    surname = EmployeeVM.Surname,
                    phone = EmployeeVM.Phone,
                    email = EmployeeVM.Email,
                    autho_id = null
                };

                if (EmployeeVM.HasSystemAccess)
                {
                    var authoId = CreateAuthoData();
                    newSupplier.autho_id = authoId;
                }

                _context.Suppliers.Add(newSupplier);
                _context.SaveChanges();
            }
            else
            {
                var newCustomer = new Customer
                {
                    last_name = EmployeeVM.LastName,
                    name = EmployeeVM.Name,
                    surname = EmployeeVM.Surname,
                    phone = EmployeeVM.Phone,
                    email = EmployeeVM.Email,
                    autho_id = null
                };

                if (EmployeeVM.HasSystemAccess)
                {
                    var authoId = CreateAuthoData();
                    newCustomer.autho_id = authoId;
                }

                _context.Customers.Add(newCustomer);
                _context.SaveChanges();
            }
        }

        private int? UpdateAuthoData(int? existingAuthoId)
        {
            if (EmployeeVM.HasSystemAccess)
            {
                if (existingAuthoId.HasValue)
                {
                    // Обновляем существующую запись авторизации
                    var autho = _context.Authoes.FirstOrDefault(a => a.id == existingAuthoId.Value);
                    if (autho != null)
                    {
                        autho.login = EmployeeVM.Login;
                        if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                        {
                            autho.password = Hash.ComputeSha256Hash(txtPassword.Password);
                        }
                        _context.SaveChanges();
                        return autho.id;
                    }
                    else
                    {
                        // Если записи нет, создаем новую
                        return CreateAuthoData();
                    }
                }
                else
                {
                    // Создаем новую запись авторизации
                    return CreateAuthoData();
                }
            }
            else if (existingAuthoId.HasValue)
            {
                // Удаляем запись авторизации, если доступ к системе отключен
                var autho = _context.Authoes.FirstOrDefault(a => a.id == existingAuthoId.Value);
                if (autho != null)
                {
                    _context.Authoes.Remove(autho);
                    _context.SaveChanges();
                }
                return null;
            }

            return existingAuthoId;
        }

        private int CreateAuthoData()
        {
            var newAutho = new Autho
            {
                login = EmployeeVM.Login,
                password = Hash.ComputeSha256Hash(txtPassword.Password)
            };

            _context.Authoes.Add(newAutho);
            _context.SaveChanges();

            return newAutho.id;
        }

        private void NotifyDataUpdated()
        {
            // Вызываем событие для обновления данных на всех подписанных страницах
            DataUpdated?.Invoke();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить сотрудника {EmployeeVM.FullName}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (EmployeeVM.Role == "Поставщик" && _originalEntity is Supplier supplier)
                    {
                        if (supplier.autho_id.HasValue)
                        {
                            var autho = _context.Authoes.FirstOrDefault(a => a.id == supplier.autho_id.Value);
                            if (autho != null)
                            {
                                _context.Authoes.Remove(autho);
                            }
                        }

                        _context.Suppliers.Remove(supplier);
                        _context.SaveChanges();
                    }
                    else if (EmployeeVM.Role == "Клиент" && _originalEntity is Customer customer)
                    {
                        if (customer.autho_id.HasValue)
                        {
                            var autho = _context.Authoes.FirstOrDefault(a => a.id == customer.autho_id.Value);
                            if (autho != null)
                            {
                                _context.Authoes.Remove(autho);
                            }
                        }

                        _context.Customers.Remove(customer);
                        _context.SaveChanges();
                    }

                    MessageBox.Show("Сотрудник успешно удален", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Вызываем событие обновления данных
                    NotifyDataUpdated();

                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Инициализация при загрузке страницы
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _context?.Dispose();
        }
    }

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

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Surname
        {
            get => _surname;
            set { _surname = value; OnPropertyChanged(); }
        }

        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Role
        {
            get => _role;
            set { _role = value; OnPropertyChanged(); }
        }

        public bool IsSupplier
        {
            get => _isSupplier;
            set { _isSupplier = value; OnPropertyChanged(); }
        }

        public bool IsCustomer
        {
            get => _isCustomer;
            set { _isCustomer = value; OnPropertyChanged(); }
        }

        public bool HasSystemAccess
        {
            get => _hasSystemAccess;
            set { _hasSystemAccess = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set { _createdDate = value; OnPropertyChanged(); }
        }

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        public int? AuthoId
        {
            get => _authoId;
            set { _authoId = value; OnPropertyChanged(); }
        }

        public object OriginalEntity
        {
            get => _originalEntity;
            set { _originalEntity = value; OnPropertyChanged(); }
        }

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public string FullName => $"{LastName} {Name} {Surname}".Trim();

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
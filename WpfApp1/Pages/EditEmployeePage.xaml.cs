using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.Models;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Pages
{
    public partial class EditEmployeePage : Page
    {
        private bool _isPhoneEditing;
        private readonly EmployeeService _service;
        private readonly string _mode;

        public EmployeeViewModel EmployeeVM { get; set; }

        public static event Action DataUpdated;

        public EditEmployeePage(Autho currentUser, string defaultRole = null)
        {
            InitializeComponent();
            _service = new EmployeeService();
            _mode = "add";

            EmployeeVM = EmployeeViewModel.CreateNew(defaultRole);
            DataContext = EmployeeVM;

            btnDelete.Visibility = Visibility.Collapsed;
        }

        public EditEmployeePage(AuthoAdmin.EmployeeViewModel employee, Autho currentUser)
        {
            InitializeComponent();
            _service = new EmployeeService();
            _mode = "edit";

            var original = _service.LoadOriginalEntity(employee.Role, employee.Id);
            EmployeeVM = EmployeeViewModel.FromExisting(employee, original);

            DataContext = EmployeeVM;
            btnDelete.Visibility = Visibility.Visible;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // 2. Проверка логина/пароля, если есть доступ к системе
                if (EmployeeVM.HasSystemAccess)
                {
                    if (string.IsNullOrWhiteSpace(EmployeeVM.Login))
                    {
                        MessageBox.Show("Введите логин", "Внимание",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (_mode == "add")
                    {
                        if (string.IsNullOrWhiteSpace(EmployeeVM.Password))
                        {
                            MessageBox.Show("Введите пароль", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (txtPassword.Password != txtPasswordConfirm.Password)
                        {
                            MessageBox.Show("Пароли не совпадают", "Внимание",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }

                // 3. Сохранение (внутри сервисов выполняется валидация Customer/Supplier)
                if (_mode == "add")
                    _service.CreateEmployee(EmployeeVM);
                else
                    _service.UpdateEmployee(EmployeeVM);

                // 4. Обновление данных и возврат
                DataUpdated?.Invoke();
                NavigationService.GoBack();
            }
            catch (ValidationException vex)
            {
                // Ошибки DataAnnotations (валидация Customer/Supplier)
                MessageBox.Show(vex.Message, "Ошибка валидации",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Удалить {EmployeeVM.FullName}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                _service.DeleteEmployee(EmployeeVM);
                DataUpdated?.Invoke();
                NavigationService.GoBack();
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _service.Dispose();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (EmployeeVM != null)
                EmployeeVM.Password = txtPassword.Password;
        }

        private void txtPasswordConfirm_PasswordChanged(object sender, RoutedEventArgs e)
        {
           
        }

        private void txtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isPhoneEditing) return;
            _isPhoneEditing = true;

            string digits = new string(txtPhone.Text.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("8"))
                digits = "7" + digits.Substring(1);

            if (!digits.StartsWith("7"))
                digits = "7" + digits;

            string formatted = "+7";

            if (digits.Length > 1)
                formatted += " (" + digits.Substring(1, Math.Min(3, digits.Length - 1));

            if (digits.Length > 4)
                formatted += ") " + digits.Substring(4, Math.Min(3, digits.Length - 4));

            if (digits.Length > 7)
                formatted += "-" + digits.Substring(7, Math.Min(2, digits.Length - 7));

            if (digits.Length > 9)
                formatted += "-" + digits.Substring(9, Math.Min(2, digits.Length - 9));

            txtPhone.Text = formatted;
            txtPhone.SelectionStart = txtPhone.Text.Length;

            EmployeeVM.Phone = formatted;

            _isPhoneEditing = false;
        }

        private void txtEmail_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.@_-";
            e.Handled = !allowed.Contains(e.Text);
        }

    }
}
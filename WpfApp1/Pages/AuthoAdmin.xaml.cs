using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp1.Models;

namespace WpfApp1.Pages
{
    public partial class AuthoAdmin : Page
    {
        private BeverageFactoryEntities _context;
        private List<EmployeeViewModel> _allEmployees = new List<EmployeeViewModel>();
        private Autho _currentUser;

        public AuthoAdmin(Autho author)
        {
            InitializeComponent();
            _currentUser = author;
            _context = new BeverageFactoryEntities();
            EditEmployeePage.DataUpdated += OnEmployeeDataUpdated;

            LoadEmployeesData();
            InitializeControls();
        }

        public AuthoAdmin()
        {
            InitializeComponent();
            _context = new BeverageFactoryEntities();
            EditEmployeePage.DataUpdated += OnEmployeeDataUpdated;

            LoadEmployeesData();
            InitializeControls();
        }

        private void InitializeControls()
        {
            txtFullName.Text = "Администратор";

            // Инициализация сортировки
            cmbSorting.Items.Add("По фамилии А-Я");
            cmbSorting.Items.Add("По фамилии Я-А");
            cmbSorting.Items.Add("По роли");
            cmbSorting.Items.Add("По дате добавления");
            cmbSorting.SelectedIndex = 0;

            // Инициализация фильтров
            cmbFilter.Items.Add("Все сотрудники");
            cmbFilter.Items.Add("Только поставщики");
            cmbFilter.Items.Add("Только клиенты");
            cmbFilter.Items.Add("С доступом");
            cmbFilter.Items.Add("Без доступа");
            cmbFilter.SelectedIndex = 0;
        }

        private void LoadEmployeesData()
        {
            try
            {
                _allEmployees.Clear();

                // Загружаем поставщиков
                var suppliers = _context.Suppliers.ToList();
                foreach (var supplier in suppliers)
                {
                    var autho = _context.Authoes.FirstOrDefault(a => a.id == supplier.autho_id);

                    _allEmployees.Add(new EmployeeViewModel
                    {
                        Id = supplier.id,
                        Name = supplier.name,
                        LastName = supplier.last_name,
                        Surname = supplier.surname,
                        Phone = supplier.phone,
                        Email = supplier.email,
                        Role = "Поставщик",
                        AuthoId = supplier.autho_id,
                        HasSystemAccess = supplier.autho_id != null && supplier.autho_id > 0,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        Position = "Поставщик продукции",
                        Department = "Отдел снабжения",
                        OriginalEntity = supplier,
                        Login = autho?.login
                    });
                }

                // Загружаем клиентов
                var customers = _context.Customers.ToList();
                foreach (var customer in customers)
                {
                    var autho = _context.Authoes.FirstOrDefault(a => a.id == customer.autho_id);

                    _allEmployees.Add(new EmployeeViewModel
                    {
                        Id = customer.id,
                        Name = customer.name,
                        LastName = customer.last_name,
                        Surname = customer.surname,
                        Phone = customer.phone,
                        Email = customer.email,
                        Role = "Клиент",
                        AuthoId = customer.autho_id,
                        HasSystemAccess = customer.autho_id != null && customer.autho_id > 0,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        Position = "Клиент",
                        Department = "Отдел продаж",
                        OriginalEntity = customer,
                        Login = autho?.login
                    });
                }

                ApplyFiltersAndSorting();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сотрудников: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnEmployeeDataUpdated()
        {
            // Этот метод вызывается при любом изменении данных сотрудника
            // Обновляем контекст базы данных
            _context?.Dispose();
            _context = new BeverageFactoryEntities();

            // Перезагружаем данные
            LoadEmployeesData();

            
            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Можно добавить визуальную индикацию обновления
            }));
        }

        private void ApplyFiltersAndSorting()
        {
            var filteredEmployees = _allEmployees.AsEnumerable();

            // Применяем поиск
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                string searchText = txtSearch.Text.ToLower();
                filteredEmployees = filteredEmployees.Where(emp =>
                    emp.FullName.ToLower().Contains(searchText) ||
                    emp.LastName.ToLower().Contains(searchText) ||
                    emp.Name.ToLower().Contains(searchText) ||
                    emp.Surname.ToLower().Contains(searchText) ||
                    emp.Phone.ToLower().Contains(searchText) ||
                    emp.Email.ToLower().Contains(searchText));
            }

            // Применяем фильтр
            if (cmbFilter.SelectedIndex >= 0)
            {
                string selectedFilter = cmbFilter.SelectedItem.ToString();
                filteredEmployees = ApplyFilter(filteredEmployees, selectedFilter);
            }

            // Применяем сортировку
            if (cmbSorting.SelectedIndex >= 0)
            {
                string selectedSort = cmbSorting.SelectedItem.ToString();
                filteredEmployees = ApplySorting(filteredEmployees, selectedSort);
            }

            // Обновляем ListView
            LViewEmployees.ItemsSource = filteredEmployees.ToList();

            // Обновляем счетчики
            UpdateCounters(filteredEmployees.Count(), _allEmployees.Count);
        }

        private IEnumerable<EmployeeViewModel> ApplyFilter(IEnumerable<EmployeeViewModel> employees, string filter)
        {
            switch (filter)
            {
                case "Только поставщики":
                    return employees.Where(e => e.Role == "Поставщик");
                case "Только клиенты":
                    return employees.Where(e => e.Role == "Клиент");
                case "С доступом":
                    return employees.Where(e => e.HasSystemAccess);
                case "Без доступа":
                    return employees.Where(e => !e.HasSystemAccess);
                default: // "Все сотрудники"
                    return employees;
            }
        }

        private IEnumerable<EmployeeViewModel> ApplySorting(IEnumerable<EmployeeViewModel> employees, string sort)
        {
            switch (sort)
            {
                case "По фамилии Я-А":
                    return employees.OrderByDescending(e => e.LastName);
                case "По роли":
                    return employees.OrderBy(e => e.Role).ThenBy(e => e.LastName);
                case "По дате добавления":
                    return employees.OrderByDescending(e => e.CreatedDate);
                default: // "По фамилии А-Я"
                    return employees.OrderBy(e => e.LastName);
            }
        }

        private void UpdateCounters(int filteredCount, int totalCount)
        {
            txtResultAmount.Text = filteredCount.ToString();
            txtAllAmount.Text = totalCount.ToString();
        }

        // Двойной клик для редактирования
        private void LViewEmployees_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LViewEmployees.SelectedItem is EmployeeViewModel selectedEmployee)
            {
                NavigationService.Navigate(new EditEmployeePage(selectedEmployee, _currentUser));
            }
        }

        // Обработчики кнопок
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditEmployeePage(_currentUser, "Поставщик"));
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployeesData();
            MessageBox.Show("Данные обновлены", "Обновление",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSorting();
        }

        private void cmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSorting();
        }

        private void cmbSorting_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSorting();
        }

        private void LViewEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public void RefreshData()
        {
            LoadEmployeesData();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // Отписываемся от события при закрытии страницы
            EditEmployeePage.DataUpdated -= OnEmployeeDataUpdated;
            _context?.Dispose();
        }

        // Обработчик для обновления при возвращении на страницу
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                // Обновляем контекст при повторном отображении страницы
                _context?.Dispose();
                _context = new BeverageFactoryEntities();
                LoadEmployeesData();
            }
        }

        public class EmployeeViewModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string LastName { get; set; }
            public string Surname { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public string RoleIcon { get; set; }
            public int? AuthoId { get; set; }
            public bool HasSystemAccess { get; set; }
            public DateTime CreatedDate { get; set; }
            public bool IsActive { get; set; }
            public string Status { get; set; }
            public string Position { get; set; }
            public string Department { get; set; }
            public object OriginalEntity { get; set; }
            public string Login { get; set; }

            public string FullName => $"{LastName} {Name} {Surname}".Trim();
            public string AuthoInfo => HasSystemAccess ? $"Логин: {Login}" : "Без доступа к системе";
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Page
    {
        int click;
        int failedAttempts;
        DispatcherTimer blockTimer;
        int blockTimeRemaining;

        public Authorization()
        {
            InitializeComponent();
            click = 0;
            failedAttempts = 0;
            InitializeBlockTimer();
        }

        private void InitializeBlockTimer()
        {
            blockTimer = new DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(1);
            blockTimer.Tick += BlockTimer;
        }

        private void BlockTimer(object sender, EventArgs e)
        {
            blockTimeRemaining--;

            if (blockTimeRemaining <= 0)
            {
                UnblockInterface();
                blockTimer.Stop();
            }
            else
            {
                txtBlockTimer.Text = $"До разблокировки осталось: {blockTimeRemaining} сек";
            }
        }

        private void BlockInterface()
        {
            // Блокируем элементы интерфейса
            btnEnter.IsEnabled = false;
            btnEnterGuests.IsEnabled = false;
            txtLogin.IsEnabled = false;
            pswbPassword.IsEnabled = false;
            txtBoxCaptcha.IsEnabled = false;

            // Показываем таймер
            txtBlockTimer.Visibility = Visibility.Visible;
            txtBlockTimer.Foreground = Brushes.Red;
            txtBlockTimer.Text = $"До разблокировки осталось: {blockTimeRemaining} сек";
        }

        private void UnblockInterface()
        {
            // Разблокируем элементы интерфейса
            btnEnter.IsEnabled = true;
            btnEnterGuests.IsEnabled = true;
            txtLogin.IsEnabled = true;
            pswbPassword.IsEnabled = true;
            txtBoxCaptcha.IsEnabled = true;

            // Скрываем таймер
            txtBlockTimer.Visibility = Visibility.Collapsed;

            // Сбрасываем счетчик неудачных попыток
            failedAttempts = 0;
        }

        private void HandleFailedAttempt()
        {
            failedAttempts++;

            if (failedAttempts >= 3)
            {
                // Блокируем интерфейс на 10 секунд
                blockTimeRemaining = 10;
                BlockInterface();
                blockTimer.Start();
            }
        }

        private void btnEnterGuests_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Client());
        }

        private void GenerateCaptcha()
        {
            txtBlockCaptcha.Visibility = Visibility.Visible;
            txtBoxCaptcha.Visibility = Visibility.Visible;

            string captchaText = CaptchaGenerator.GenerateCaptchaText(6);
            txtBlockCaptcha.Text = captchaText;
            txtBlockCaptcha.TextDecorations = TextDecorations.Strikethrough;
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, не заблокирован ли интерфейс
            if (!btnEnter.IsEnabled)
                return;

            click += 1;
            string login = txtLogin.Text.Trim();
            string password = pswbPassword.Password.Trim();

            // Проверяем, что поля не пустые
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            using (var db = new BeverageFactoryEntities())
            {
                // Находим пользователя по логину
                var author = db.Authoes.Where(x => x.login == login).FirstOrDefault();

                if (author != null)
                {
                    // Хэшируем введенный пароль и сравниваем с хэшем в базе
                    string hashedPassword = Hash.ComputeSha256Hash(password);
                    bool isPasswordValid = author.password == hashedPassword;

                    // ПРОВЕРКА НА АДМИНИСТРАТОРА ПО ID
                    if (IsAdminId(author.id))
                    {
                        if (isPasswordValid)
                        {
                            MessageBox.Show("Вы успешно авторизовались как администратор!");
                            failedAttempts = 0; // Сбрасываем счетчик при успешной авторизации

                            // Открываем страницу администратора
                            LoadPage("Admin", author, null);
                            return; // Выходим из метода, так как администратор авторизован
                        }
                        else
                        {
                            HandleFailedAttempt();
                            MessageBox.Show("Неверный пароль для администратора!");
                            GenerateCaptcha();
                            pswbPassword.Password = "";
                            return;
                        }
                    }

                    if (click == 1)
                    {
                        if (isPasswordValid)
                        {
                            if (!CheckWorkingHoursForEmployee(author.id, db))
                            {
                                // Рабочее время не соблюдается, прерываем авторизацию
                                return;
                            }

                            MessageBox.Show("Вы успешно авторизовались!");
                            failedAttempts = 0; // Сбрасываем счетчик при успешной авторизации

                            // Проверяем, является ли пользователь поставщиком или клиентом
                            var supplier = db.Suppliers.FirstOrDefault(s => s.autho_id == author.id);
                            var customer = db.Customers.FirstOrDefault(c => c.autho_id == author.id);

                            if (supplier != null)
                            {
                                LoadPage("Supplier", author, supplier);
                            }
                            else if (customer != null)
                            {
                                LoadPage("Customer", author, customer);
                            }
                            else
                            {
                                NavigationService.Navigate(new Client());
                            }
                        }
                        else
                        {
                            HandleFailedAttempt();
                            MessageBox.Show("Вы ввели логин или пароль неверно!");
                            GenerateCaptcha();
                            pswbPassword.Password = "";
                        }
                    }
                    else if (click > 1)
                    {
                        if (isPasswordValid && txtBoxCaptcha.Text.Trim() == txtBlockCaptcha.Text)
                        {
                            if (!CheckWorkingHoursForEmployee(author.id, db))
                            {
                                // Рабочее время не соблюдается, прерываем авторизацию
                                return;
                            }

                            MessageBox.Show("Вы успешно авторизовались!");
                            failedAttempts = 0; // Сбрасываем счетчик при успешной авторизации

                            var supplier = db.Suppliers.FirstOrDefault(s => s.autho_id == author.id);
                            var customer = db.Customers.FirstOrDefault(c => c.autho_id == author.id);

                            if (supplier != null)
                            {
                                LoadPage("Supplier", author, supplier);
                            }
                            else if (customer != null)
                            {
                                LoadPage("Customer", author, customer);
                            }
                            else
                            {
                                NavigationService.Navigate(new Client());
                            }
                        }
                        else
                        {
                            HandleFailedAttempt();
                            MessageBox.Show("Неверные данные или капча! Введите данные заново!");
                            pswbPassword.Password = "";
                            txtBoxCaptcha.Text = "";
                            GenerateCaptcha();
                        }
                    }
                }
                else
                {
                    if (click == 1)
                    {
                        HandleFailedAttempt();
                        MessageBox.Show("Пользователь с таким логином не найден!");
                        GenerateCaptcha();
                        pswbPassword.Password = "";
                    }
                    else if (click > 1)
                    {
                        if (txtBoxCaptcha.Text.Trim() == txtBlockCaptcha.Text)
                        {
                            HandleFailedAttempt();
                            MessageBox.Show("Пользователь с таким логином не найден!");
                        }
                        else
                        {
                            HandleFailedAttempt();
                            MessageBox.Show("Неверная капча! Введите данные заново!");
                        }
                        pswbPassword.Password = "";
                        txtBoxCaptcha.Text = "";
                        GenerateCaptcha();
                    }
                }
            }
        }

        // Метод для проверки ID администратора
        private bool IsAdminId(int id)
        {
            int adminId = 1;


            return id == adminId;
        }

        private bool CheckWorkingHoursForEmployee(int authoId, BeverageFactoryEntities db)
        {
            // Проверяем, является ли пользователь сотрудником (Customer или Supplier)
            var isCustomer = db.Customers.Any(c => c.autho_id == authoId);
            var isSupplier = db.Suppliers.Any(s => s.autho_id == authoId);

            // Если это не сотрудник (ни Customer, ни Supplier), разрешаем доступ в любое время
            if (!isCustomer && !isSupplier)
            {
                return true;
            }

            // Если это сотрудник, проверяем рабочее время
            if (!TimeHelper.IsWithinWorkingHours())
            {
                MessageBox.Show(
                    $"Доступ к системе разрешен только в рабочее время (с 10:00 до 19:00).\n" +
                    $"Текущее время: {DateTime.Now:HH:mm}\n" +
                    $"Пожалуйста, вернитесь позже.",
                    "Доступ запрещен",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return false;
            }

            return true;
        }

        private void LoadPage(string role, Autho author, dynamic userData)
        {
            click = 0;
            failedAttempts = 0; // Сбрасываем счетчик при успешной авторизации

            // Сбрасываем капчу
            txtBlockCaptcha.Visibility = Visibility.Collapsed;
            txtBoxCaptcha.Visibility = Visibility.Collapsed;
            txtBoxCaptcha.Text = "";

            switch (role)
            {
                case "Admin":
                    // Открываем страницу администратора
                    NavigationService.Navigate(new AuthoAdmin(author));
                    break;
                case "Supplier":
                    NavigationService.Navigate(new AuthoSupplier(author, (Supplier)userData));
                    break;
                case "Customer":
                    NavigationService.Navigate(new AuthoCustomer(author, (Customer)userData));
                    break;
                default:
                    NavigationService.Navigate(new Client());
                    break;
            }
        }
    }
}
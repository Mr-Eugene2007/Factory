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
using WpfApp1.Models;

namespace WpfApp1.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthoCustomer.xaml
    /// </summary>
    public partial class AuthoCustomer : Page
    {
            public AuthoCustomer(Autho author, Customer customerData)
            {
                InitializeComponent();

                if (author != null && customerData != null)
                {
                    // Формируем полное имя
                    string fullName = $"{customerData.last_name} {customerData.name} {customerData.surname}".Replace("  ", " ").Trim();

                    // Обновляем текстовые блоки
                    txtWelcome.Text = $"Добро пожаловать, {customerData.name}!";
                    txtFullName.Text = fullName;
                    txtEmail.Text = customerData.email ?? "Не указан";
                    txtPhone.Text = customerData.phone ?? "Не указан";
                }
                else
                {
                    txtWelcome.Text = "Добро пожаловать!";
                    txtFullName.Text = "Информация недоступна";
                    txtEmail.Text = "Информация недоступна";
                    txtPhone.Text = "Информация недоступна";
                }
            }
    }
}

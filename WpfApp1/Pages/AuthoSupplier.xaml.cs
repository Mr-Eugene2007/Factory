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
    /// Логика взаимодействия для AuthoSupplier.xaml
    /// </summary>
    public partial class AuthoSupplier : Page
    {
        public AuthoSupplier(Autho author, Supplier supplierData)
        {
            InitializeComponent();

            if (author != null && supplierData != null)
            {
                // Формируем полное имя
                string fullName = $"{supplierData.last_name} {supplierData.name} {supplierData.surname}".Replace("  ", " ").Trim();

                // Обновляем текстовые блоки
                txtWelcome.Text = $"Добро пожаловать, {supplierData.name}!";
                txtFullName.Text = fullName;
                txtEmail.Text = supplierData.email ?? "Не указан";
                txtPhone.Text = supplierData.phone ?? "Не указан";
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

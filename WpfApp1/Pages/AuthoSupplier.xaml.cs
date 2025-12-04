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
using WpfApp1.Services;

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
                // Получаем гендер из базы данных
                Gender gender = null;
                using (var db = new BeverageFactoryEntities())
                {
                    gender = db.Genders.FirstOrDefault(g => g.id == supplierData.gender_id);
                }

                // Формируем полное имя с приставкой Mr/Mrs
                string fullName = TimeHelper.GetFullNameWithGender(
                    supplierData.last_name,
                    supplierData.name,
                    supplierData.surname,
                    gender
                );

                // Получаем полное приветствие с учетом времени суток и гендера
                string greeting = TimeHelper.GetFullGreeting(
                    supplierData.last_name,
                    supplierData.name,
                    supplierData.surname,
                    gender
                );

                // Обновляем текстовые блоки
                txtWelcome.Text = greeting;
            }
            else
            {
                txtWelcome.Text = "Добро пожаловать!";
            }
        }
    }
}

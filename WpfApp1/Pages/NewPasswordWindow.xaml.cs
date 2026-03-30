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
using System.Windows.Shapes;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Pages
{
    /// <summary>
    /// Логика взаимодействия для NewPasswordWindow.xaml
    /// </summary>
    public partial class NewPasswordWindow : Window
    {
        private string login;

        public NewPasswordWindow(string login)
        {
            InitializeComponent();
            this.login = login;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (psw1.Password != psw2.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            using (var db = new BeverageFactoryEntities())
            {
                var user = db.Authoes.FirstOrDefault(u => u.login == login);

                if (user != null)
                {
                    user.password = Hash.ComputeSha256Hash(psw1.Password);
                    db.SaveChanges();
                }
            }

            MessageBox.Show("Пароль успешно изменён!");
            Close();
        }
    }

}

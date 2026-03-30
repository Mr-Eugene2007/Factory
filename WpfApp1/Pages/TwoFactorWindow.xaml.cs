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
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace WpfApp1.Pages
{
    /// <summary>
    /// Логика взаимодействия для TwoFactorWindow.xaml
    /// </summary>
    public partial class TwoFactorWindow : Window
    {
        private string correctCode;
        public bool IsConfirmed { get; private set; }

        public TwoFactorWindow(string code)
        {
            InitializeComponent();
            correctCode = code;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (txtCode.Text.Trim() == correctCode)
            {
                IsConfirmed = true;
                Close();
            }
            else
            {
                MessageBox.Show("Код неверный!");
            }
        }
    }
}

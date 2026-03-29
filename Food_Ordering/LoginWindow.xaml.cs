using Food_Ordering.Entities;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Food_Ordering
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {

        FoodOrderingDbContext _DB;
        public LoginWindow()
        {
            InitializeComponent();
            _DB = new();
            List<string> list = new();
            list.Add("Customer");
            list.Add("Shipper");
            list.Add("Restaurant");
            cbRegRole.ItemsSource = list;
            cbRegRole.SelectedIndex = 0;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (txtEmail == null)
            {
                MessageBox.Show("Email cannot empty!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtPassword == null)
            {
                MessageBox.Show("Please enter password!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var account = _DB.Users.FirstOrDefault(x => x.Email.ToLower().Equals(txtEmail.Text.ToLower())
                                                    && x.PasswordHash.Equals(txtPassword));
            if (account != null)
            {
                switch (account.Role)
                {
                    case "Admin":

                        break;
                    case "Customer":

                        break;
                    case "Shipper":

                        break;
                    case "Restaurant":

                        break;
                }
            }
            else
            {
                MessageBox.Show("Invalid Email or Password",
                    "ERROR",
                    MessageBoxButton.OK);
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (txtRegEmail == null)
            {
                MessageBox.Show("Email cannot empty!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtRegPassword == null)
            {
                MessageBox.Show("Email cannot empty!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtRegConfirmPassword == null)
            {
                MessageBox.Show("Email cannot empty!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!txtRegPassword.Equals(txtRegConfirmPassword))
            {
                MessageBox.Show("Email cannot empty!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtRegPhone == null)
            {
                MessageBox.Show("Email cannot empty!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var account = new User
            {
                Email = txtRegEmail.Text,
                PasswordHash = txtRegPassword.ToString(),
                FullName = txtRegFullname.Text,
                PhoneNumber = txtRegPhone.Text,
                Status = "Available",
                Role = (string)cbRegRole.SelectedItem,
                CreatedAt = DateTime.Now,
            };

        }
    }
}
using Food_Ordering.Entities;
using Microsoft.IdentityModel.Tokens;
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
            list.Add("RestaurantOwner");
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
                    case "RestaurantOwner":

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
            if (txtRegEmail.Text.IsNullOrEmpty())
            {
                MessageBox.Show("Email cannot empty!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtRegPassword.Password.IsNullOrEmpty())
            {
                MessageBox.Show("password cannot empty!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtRegConfirmPassword.Password.IsNullOrEmpty())
            {
                MessageBox.Show("Need to confirm password", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!(txtRegPassword.Password.Equals(txtRegConfirmPassword.Password)))
            {
                MessageBox.Show("Password is not the same", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtRegPhone.Text.IsNullOrEmpty())
            {
                MessageBox.Show("Phone number cannot empty!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var account = new User
            {
                Email = txtRegEmail.Text,
                PasswordHash = txtRegPassword.Password,
                FullName = txtRegFullname.Text,
                PhoneNumber = txtRegPhone.Text,
                Status = "Active",
                Role = (string)cbRegRole.SelectedItem,
                CreatedAt = DateTime.Now,
            };

            _DB.Users.Add(account);
            _DB.SaveChanges();

            switch (account.Role)
            {
                case "Customer":
                    var customer = new Customer
                    {
                        CustomerId = account.UserId,
                        Address = txtRegAdress.Text,
                    };
                    _DB.Customers.Add(customer);
                    break;
                case "Shipper":
                    var shipper = new Shipper
                    {
                        ShipperId = account.UserId,
                        VehicleType = "motorbike",
                        LicensePlate = txtLicensePlate.Text,
                        IsAvailable = true,
                    };
                    _DB.Shippers.Add(shipper);
                    break;
                case "RestaurantOwner":
                    var restaurantOwner = new RestaurantOwner
                    {
                        OwnerId = account.UserId,
                    };
                    _DB.Add(restaurantOwner);
                    break;
            }
            _DB.SaveChanges();
            MessageBox.Show("Register successfully!", "SUCCESS", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void cbRegRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((string)cbRegRole.SelectedItem)
            {
                case "Customer":
                    grShipper.Visibility = Visibility.Collapsed;
                    grCustomer.Visibility = Visibility.Visible;
                    break;
                case "Shipper":
                    grCustomer.Visibility = Visibility.Collapsed;
                    grShipper.Visibility = Visibility.Visible;
                    break;
                default:
                    grCustomer.Visibility = Visibility.Collapsed;
                    grShipper.Visibility = Visibility.Collapsed;
                    return;
            }
        }
    }
}
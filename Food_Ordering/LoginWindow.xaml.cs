using Food_Ordering.Entities;
using Food_Ordering.shipper;
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
            if (txtEmail.Text.IsNullOrEmpty())
            {
                MessageBox.Show("Email cannot empty!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtPassword.Password.IsNullOrEmpty())
            {
                MessageBox.Show("Please enter password!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var account = _DB.Users.FirstOrDefault(x => x.Email.ToLower().Equals(txtEmail.Text.ToLower())
                                                    && x.PasswordHash.Equals(txtPassword.Password));

            if (account != null)
            {
                switch (account.Role)
                {
                    case "Admin":
                        Food_Ordering.Admin.AdminWindow adminWindow = new Food_Ordering.Admin.AdminWindow(account);
                        adminWindow.Show();
                        this.Close();
                        break;
                    case "Customer":
                        MainWindow mainWindow = new MainWindow(account);
                        mainWindow.Show();
                        this.Close();
                        break;
                    case "Shipper":
                        Food_Ordering.shipper.Shipper shipper = new Food_Ordering.shipper.Shipper(account);
                        shipper.Show();
                        this.Close();
                        break;
                    case "RestaurantOwner":
                        RestaurantWindow restaurantWindow = new RestaurantWindow(account);
                        restaurantWindow.Show();
                        this.Close();
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
                    var shipper = new Food_Ordering.Entities.Shipper
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
            
            // Switch to Login tab
            TabControl tabControl = (TabControl)this.FindName("TabControl");
            if (tabControl != null)
            {
                tabControl.SelectedIndex = 0; // 0 = Login tab, 1 = Register tab
            }
            
            // Clear registration form fields
            txtRegEmail.Clear();
            txtRegPassword.Clear();
            txtRegConfirmPassword.Clear();
            txtRegFullname.Clear();
            txtRegPhone.Clear();
            txtLicensePlate.Clear();
            txtRegAdress.Clear();
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
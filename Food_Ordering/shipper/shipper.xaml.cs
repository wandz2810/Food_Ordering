using Food_Ordering.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Food_Ordering.shipper
{
    public partial class Shipper : Window
    {
        private readonly string _connectionString;
        private User? _currentAccount;

        public Shipper()
        {
            InitializeComponent();
            _connectionString = GetConnectionString();
            LoadOrders();
        }

        public Shipper(User currentAccount)
        {
            InitializeComponent();
            _connectionString = GetConnectionString();
            _currentAccount = currentAccount;
            LoadOrders();
        }

        private string GetConnectionString()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string? connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("DefaultConnection not found in appsettings.json");
            }

            return connectionString;
        }

        private void LoadOrders()
        {
            using FoodOrderingDbContext context = new FoodOrderingDbContext();

            dgOrders.ItemsSource = context.Orders
                                          .OrderBy(o => o.OrderId)
                                          .ToList();
        }

        private void dgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrders.SelectedItem is not Order selectedOrder)
            {
                return;
            }

            txtOrderId.Text = selectedOrder.OrderId.ToString();
            txtRestaurantId.Text = selectedOrder.RestaurantId.ToString();
            txtTotalAmount.Text = selectedOrder.TotalAmount.ToString();
            txtDeliveryAddress.Text = selectedOrder.DeliveryAddress ?? "";
            txtNote.Text = selectedOrder.Note ?? "";
        }

        private void btnReceive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtOrderId.Text))
                {
                    MessageBox.Show("Please select order!!");
                    return;
                }

                if (!int.TryParse(txtOrderId.Text, out int orderId))
                {
                    MessageBox.Show("OrderId is invalid.");
                    return;
                }

                shipperAccept acceptWindow = new shipperAccept(orderId);
                acceptWindow.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
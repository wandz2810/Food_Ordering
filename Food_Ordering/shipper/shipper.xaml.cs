using Food_Ordering.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Food_Ordering.shipper
{
    public partial class Shipper : Window
    {
        private readonly string _connectionString;

        public Shipper()
        {
            InitializeComponent();
            _connectionString = GetConnectionString();
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
            using FoodOrderingDbContext context = new FoodOrderingDbContext(_connectionString);
            dgOrders.ItemsSource = context.Orders
                                          .OrderBy(e => e.OrderId)
                                          .ToList();
        }

        private void ClearInput()
        {
            txtOrderId.Text = string.Empty;
            txtRestaurantId.Text = string.Empty;
            txtTotalAmount.Text = string.Empty;
            txtDeliveryAddress.Text = string.Empty;
            txtNote.Text = string.Empty;
            dgOrders.SelectedIndex = -1;
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
            txtDeliveryAddress.Text = selectedOrder.DeliveryAddress ?? string.Empty;
            txtNote.Text = selectedOrder.Note ?? string.Empty;
        }
    }
}
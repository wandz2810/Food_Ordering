using Food_Ordering.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Food_Ordering.shipper
{

    public partial class shipper : Window
    {

        private readonly string _connectionString = string.Empty;
        private int _selectedEmployeeId = 0;
        public shipper()
        {
            InitializeComponent();
            _connectionString = GetConnectionString();
            LoadEmployees();        }


        private string GetConnectionString()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            return configuration.GetConnectionString("DefaultConnection")!;
        }


         

        private void LoadEmployees()
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
            if (dgOrders.SelectedItem == null)
            {
                ClearInput();
                return;
            }

            dynamic selectedOrder = dgOrders.SelectedItem;

            txtOrderId.Text = selectedOrder.OrderId?.ToString() ?? "";
            txtRestaurantId.Text = selectedOrder.RestaurantId?.ToString() ?? "";
            txtTotalAmount.Text = selectedOrder.TotalAmount?.ToString() ?? "";
            txtDeliveryAddress.Text = selectedOrder.DeliveryAddress?.ToString() ?? "";
            txtNote.Text = selectedOrder.Note?.ToString() ?? "";
        }
    }




}

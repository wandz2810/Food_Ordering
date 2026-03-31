using Food_Ordering.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Food_Ordering
{
    public partial class HistoryWindow : Window
    {
        private readonly FoodOrderingDbContext _context = new FoodOrderingDbContext();
        private readonly User _user;

        public HistoryWindow(User user)
        {
            InitializeComponent();
            _user = user ?? throw new ArgumentNullException(nameof(user));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var myOrders = _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Restaurant)
                    .Include(o => o.Shipper)
                    .Where(o => o.CustomerId == _user.UserId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();

                dgOrders.ItemsSource = myOrders;
                lbOrderDetails.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải lịch sử: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void DgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order selectedOrder)
            {
                var details = _context.OrderItems
                    .AsNoTracking()
                    .Where(oi => oi.OrderId == selectedOrder.OrderId)
                    .ToList();

                lbOrderDetails.ItemsSource = details;
            }
        }

        
    }
}
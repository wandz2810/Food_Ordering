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
        private User _user;

        // Constructor nhận đối tượng User
        public HistoryWindow(User user)
        {
            InitializeComponent();
            _user = user;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                // Lọc theo UserId truyền vào
                int currentId = _user.UserId;

                var myOrders = _context.Orders
                    .Include(o => o.Restaurant)
                    .Include(o => o.Shipper)
                    .Where(o => o.CustomerId == currentId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();

                dgOrders.ItemsSource = myOrders;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử: " + ex.Message);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            MessageBox.Show("Đã cập nhật dữ liệu mới nhất!");
        }

        private void DgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order selectedOrder)
            {
                var details = _context.OrderItems
                    .Where(oi => oi.OrderId == selectedOrder.OrderId)
                    .ToList();
                lbOrderDetails.ItemsSource = details;
            }
        }

        private void BtnCancelOrder_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = ((Button)sender).DataContext as Order;
            if (selectedOrder != null && (selectedOrder.Status == "Pending" || selectedOrder.Status == "WaitingShipper"))
            {
                if (MessageBox.Show("Hủy đơn hàng này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var order = _context.Orders.Find(selectedOrder.OrderId);
                    order.Status = "Cancelled";
                    order.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                    LoadOrders();
                }
            }
        }

        private void BtnReceived_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = ((Button)sender).DataContext as Order;
            if (selectedOrder != null && (selectedOrder.Status == "Shipping" || selectedOrder.Status == "WaitingShipper"))
            {
                if (MessageBox.Show("Xác nhận đã nhận hàng?", "Hoàn thành", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var order = _context.Orders.Find(selectedOrder.OrderId);
                    order.Status = "Completed";
                    order.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();
                    LoadOrders();
                }
            }
        }
    }
}
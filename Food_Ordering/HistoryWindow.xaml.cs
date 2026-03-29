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

        public HistoryWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        // Hàm load dữ liệu dùng chung cho cả Window_Loaded và nút Refresh
        private void LoadOrders()
        {
            try
            {
                int currentId = App.CurrentUserId;

                // Load đơn hàng kèm theo thông tin Nhà hàng và Shipper
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
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        // Nút Làm mới trạng thái
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            MessageBox.Show("Đã cập nhật trạng thái mới nhất từ hệ thống!", "Thông báo");
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

        // --- HÀNH ĐỘNG HỦY ĐƠN ---
        private void BtnCancelOrder_Click(object sender, RoutedEventArgs e)
        {
            // Lấy dữ liệu của dòng chứa nút bấm
            var selectedOrder = ((Button)sender).DataContext as Order;

            if (selectedOrder != null)
            {
                // Chỉ cho phép hủy khi đơn hàng mới tạo (Pending)
                if (selectedOrder.Status == "Pending" || selectedOrder.Status == "WaitingShipper")
                {
                    var result = MessageBox.Show("Bạn có chắc muốn hủy đơn hàng này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        var orderToUpdate = _context.Orders.Find(selectedOrder.OrderId);
                        orderToUpdate.Status = "Cancelled";
                        orderToUpdate.UpdatedAt = DateTime.Now;
                        _context.SaveChanges();

                        MessageBox.Show("Đã hủy đơn hàng thành công.");
                        LoadOrders();
                    }
                }
                else
                {
                    MessageBox.Show("Nhà hàng đã nhận đơn hoặc hàng đang giao, không thể hủy!");
                }
            }
        }

        // --- HÀNH ĐỘNG XÁC NHẬN ĐÃ NHẬN HÀNG ---
        private void BtnReceived_Click(object sender, RoutedEventArgs e)
        {
            var selectedOrder = ((Button)sender).DataContext as Order;

            if (selectedOrder != null)
            {
                // Cho phép xác nhận khi đơn đang giao hoặc chờ shipper
                if (selectedOrder.Status == "Shipping" || selectedOrder.Status == "WaitingShipper")
                {
                    var result = MessageBox.Show("Xác nhận bạn đã nhận được món ăn này?", "Hoàn thành đơn hàng", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        var orderToUpdate = _context.Orders.Find(selectedOrder.OrderId);
                        orderToUpdate.Status = "Completed";
                        orderToUpdate.UpdatedAt = DateTime.Now;
                        _context.SaveChanges();

                        MessageBox.Show("Cảm ơn bạn đã sử dụng dịch vụ!");
                        LoadOrders();
                    }
                }
                else if (selectedOrder.Status == "Completed")
                {
                    MessageBox.Show("Đơn hàng này đã hoàn thành!");
                }
                else
                {
                    MessageBox.Show("Đơn hàng chưa được giao đến nơi!");
                }
            }
        }
    }
}
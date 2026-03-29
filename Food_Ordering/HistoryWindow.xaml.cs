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
        // Sử dụng một context riêng cho Window này để tránh xung đột dữ liệu
        private readonly FoodOrderingDbContext _context = new FoodOrderingDbContext();
        private readonly User _user;

        public HistoryWindow(User user)
        {
            InitializeComponent();
            // Kiểm tra nếu user bị null thì báo lỗi ngay lập tức
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
                // Sử dụng AsNoTracking() vì đây là thao tác chỉ đọc (Read-only), giúp tăng hiệu năng
                var myOrders = _context.Orders
                    .AsNoTracking()
                    .Include(o => o.Restaurant)
                    .Include(o => o.Shipper)
                    .Where(o => o.CustomerId == _user.UserId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToList();

                dgOrders.ItemsSource = myOrders;

                // Nếu không có đơn hàng nào, có thể xóa trắng phần chi tiết
                lbOrderDetails.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải lịch sử: {ex.Message}", "Lỗi SQL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders();
            // Chỉ thông báo ở thanh trạng thái hoặc dùng tooltip thay vì MessageBox liên tục (gây phiền cho user)
        }

        private void DgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order selectedOrder)
            {
                // Tải chi tiết món ăn của đơn hàng đang chọn
                var details = _context.OrderItems
                    .AsNoTracking()
                    .Where(oi => oi.OrderId == selectedOrder.OrderId)
                    .ToList();

                lbOrderDetails.ItemsSource = details;
            }
        }

        private void BtnCancelOrder_Click(object sender, RoutedEventArgs e)
        {
            // Lấy object Order gắn với dòng chứa nút Hủy
            if ((sender as Button)?.DataContext is Order selectedOrder)
            {
                // Chỉ cho phép hủy khi đang ở trạng thái chờ
                if (selectedOrder.Status == "Pending" || selectedOrder.Status == "WaitingShipper")
                {
                    var result = MessageBox.Show("Bạn có chắc chắn muốn hủy đơn hàng này không?",
                        "Xác nhận hủy", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        UpdateOrderStatus(selectedOrder.OrderId, "Cancelled");
                    }
                }
                else
                {
                    MessageBox.Show("Đơn hàng đã được xử lý, không thể hủy lúc này!", "Thông báo");
                }
            }
        }

        private void BtnReceived_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Order selectedOrder)
            {
                // Chỉ cho nhận khi hàng đang giao
                if (selectedOrder.Status == "Shipping" || selectedOrder.Status == "WaitingShipper")
                {
                    var result = MessageBox.Show("Xác nhận bạn đã nhận đủ món ăn và thanh toán?",
                        "Hoàn thành đơn", MessageBoxButton.YesNo, MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        UpdateOrderStatus(selectedOrder.OrderId, "Completed");
                    }
                }
            }
        }

        // Hàm dùng chung để cập nhật trạng thái đơn hàng (Clean Code)
        private void UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                var order = _context.Orders.Find(orderId);
                if (order != null)
                {
                    order.Status = newStatus;
                    order.UpdatedAt = DateTime.Now;

                    _context.SaveChanges();

                    // Sau khi lưu xong, load lại danh sách để UI cập nhật trạng thái mới
                    LoadOrders();
                    MessageBox.Show($"Đơn hàng đã được chuyển sang trạng thái: {newStatus}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}");
            }
        }
    }
}
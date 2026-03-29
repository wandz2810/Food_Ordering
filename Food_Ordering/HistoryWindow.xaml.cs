using Food_Ordering.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

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

        private void LoadOrders()
        {
            int currentId = App.CurrentUserId;

            // Dùng .Include để lấy thêm Tên Nhà Hàng và Tên Shipper
            var myOrders = _context.Orders
                .Include(o => o.Restaurant) 
                .Include(o => o.Shipper)
                .Where(o => o.CustomerId == currentId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            dgOrders.ItemsSource = myOrders;
        }

        private void DgOrders_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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
            if (dgOrders.SelectedItem is Order selectedOrder)
            {
                if (selectedOrder.Status == "Pending" || selectedOrder.Status == "WaitingShipper")
                {
                    var result = MessageBox.Show("Bạn có chắc muốn hủy đơn hàng này?", "Xác nhận", MessageBoxButton.YesNo);
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
                    MessageBox.Show("Không thể hủy đơn hàng đang trong quá trình chuẩn bị hoặc giao!");
                }
            }
        }

        // --- HÀNH ĐỘNG XÁC NHẬN ĐÃ NHẬN HÀNG ---
        private void BtnReceived_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order selectedOrder)
            {
                // Thông thường khách xác nhận khi đang Shipping
                if (selectedOrder.Status == "Shipping" || selectedOrder.Status == "WaitingShipper") 
                {
                    var orderToUpdate = _context.Orders.Find(selectedOrder.OrderId);
                    orderToUpdate.Status = "Completed";
                    orderToUpdate.UpdatedAt = DateTime.Now;
                    _context.SaveChanges();

                    MessageBox.Show("Cảm ơn bạn đã mua hàng! Đơn hàng đã hoàn thành.");
                    LoadOrders();
                }
                else if (selectedOrder.Status == "Completed")
                {
                    MessageBox.Show("Đơn hàng này đã hoàn thành rồi!");
                }
                else
                {
                    MessageBox.Show("Đơn hàng chưa được giao nên không thể xác nhận!");
                }
            }
        }
    }
}
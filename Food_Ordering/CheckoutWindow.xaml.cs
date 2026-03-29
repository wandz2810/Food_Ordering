using Food_Ordering.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Food_Ordering
{
    public partial class CheckoutWindow : Window
    {
        private readonly FoodOrderingDbContext _context = new FoodOrderingDbContext();
        private List<FoodItem> _selectedItems;

        public CheckoutWindow(List<FoodItem> items)
        {
            InitializeComponent();
            _selectedItems = items;

            // Hiển thị danh sách món và tính tổng tiền
            lbSummary.ItemsSource = _selectedItems;
            txtTotal.Text = $"{_selectedItems.Sum(x => x.Price):N0} VNĐ";

            // Điền sẵn thông tin mẫu cho Liem test
            txtFullName.Text = "Nguyen Van Khach";
            txtEmail.Text = "customer@test.com";
            txtPhone.Text = "0901234567";
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Bước 1: Kiểm tra nhập liệu (Validation)
            if (string.IsNullOrWhiteSpace(txtAddress.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Số điện thoại và Địa chỉ giao hàng!", "Thông báo");
                return;
            }

            try
            {
                // Bước 2: Tạo bản ghi Order
                var newOrder = new Order
                {
                    // SỬA Ở ĐÂY: Lấy ID từ App chứ không dùng số 2 nữa
                    CustomerId = App.CurrentUserId,

                    RestaurantId = _selectedItems.First().RestaurantId,
                    TotalAmount = _selectedItems.Sum(x => x.Price),
                    DeliveryAddress = txtAddress.Text,
                    Status = "WaitingShipper",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    PaymentMethod = "Cash",
                    Note = $"SĐT: {txtPhone.Text}"
                };

                _context.Orders.Add(newOrder);
                _context.SaveChanges();

                // Bước 3: Lưu chi tiết món (Giữ nguyên logic foreach của Liem)
                foreach (var item in _selectedItems)
                {
                    var orderDetail = new OrderItem
                    {
                        OrderId = newOrder.OrderId,
                        FoodItemId = item.FoodItemId,
                        FoodName = item.Name,
                        FoodPrice = item.Price,
                        Quantity = 1
                    };
                    _context.OrderItems.Add(orderDetail);
                }

                _context.SaveChanges();

                MessageBox.Show($"Đặt hàng thành công!\nĐơn hàng #{newOrder.OrderId} đã được tạo.", "Thành công");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}

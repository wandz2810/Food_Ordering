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
        private User _user;

        public CheckoutWindow(List<FoodItem> items, User user)
        {
            InitializeComponent();
            _selectedItems = items;
            _user = user;

            // Hiển thị danh sách và tính tổng tiền (Giá * Số lượng)
            lbSummary.ItemsSource = _selectedItems;
            decimal total = _selectedItems.Sum(x => x.Price * x.Quantity);
            txtTotal.Text = $"{total:N0} VNĐ";

            // Tự động điền thông tin từ object User
            txtFullName.Text = _user.FullName;
            txtEmail.Text = _user.Email;
            txtPhone.Text = _user.PhoneNumber;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Bước 1: Kiểm tra địa chỉ
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ giao hàng!", "Thông báo");
                return;
            }

            // Bước 2: Kiểm tra danh sách món ăn để tránh lỗi .First()
            if (_selectedItems == null || !_selectedItems.Any())
            {
                MessageBox.Show("Giỏ hàng trống, không thể thanh toán!");
                return;
            }

            try
            {
                // Bước 3: Tạo đối tượng Order
                var newOrder = new Order
                {
                    CustomerId = _user.UserId,
                    RestaurantId = _selectedItems.First().RestaurantId,
                    TotalAmount = _selectedItems.Sum(x => x.Price * x.Quantity),
                    DeliveryAddress = txtAddress.Text,
                    Status = "WaitingShipper",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    PaymentMethod = "Cash",
                    Note = $"SĐT: {txtPhone.Text}"
                };

                // Thêm đơn hàng vào Context nhưng CHƯA SaveChanges vội
                _context.Orders.Add(newOrder);

                // Bước 4: Tạo danh sách OrderItems
                foreach (var item in _selectedItems)
                {
                    var orderDetail = new OrderItem
                    {
                        // Gán trực tiếp đối tượng newOrder để EF tự hiểu mối quan hệ
                        Order = newOrder,
                        FoodItemId = item.FoodItemId,
                        FoodName = item.Name,
                        FoodPrice = item.Price,
                        Quantity = item.Quantity
                    };
                    _context.OrderItems.Add(orderDetail);
                }

                // Bước 5: Lưu TẤT CẢ vào Database trong một lượt duy nhất
                // Nếu có bất kỳ lỗi nào ở bước này, toàn bộ quá trình sẽ hủy bỏ (Transaction)
                _context.SaveChanges();

                MessageBox.Show($"Đặt hàng thành công! Mã đơn: #{newOrder.OrderId}", "Thành công");

                // Trả về kết quả thành công để MainWindow biết đường xóa sạch giỏ hàng
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                // In ra chi tiết lỗi để dễ debug
                MessageBox.Show("Lỗi hệ thống khi lưu đơn hàng: " + ex.Message);
            }
        }
    }
}
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
                // Bước 2: Tạo bản ghi Order (Bảng Cha)
                var newOrder = new Order
                {
                    CustomerId = 2, // Phải khớp với ID trong bảng Customers của SQL
                    RestaurantId = _selectedItems.First().RestaurantId,
                    TotalAmount = _selectedItems.Sum(x => x.Price),
                    DeliveryAddress = txtAddress.Text,
                    Status = "WaitingShipper", // Trạng thái gửi cho Shipper
                    ShipperId = null, 
                    PaymentMethod = "Cash",
                    Note = $"SĐT: {txtPhone.Text}",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Orders.Add(newOrder);
                _context.SaveChanges(); // Lưu lần 1: Để SQL sinh ra OrderId

                // Bước 3: Lưu chi tiết từng món ăn (Bảng Con)
                // LƯU Ý: Phải chạy hết vòng lặp này TRƯỚC khi đóng cửa sổ
                foreach (var item in _selectedItems)
                {
                    var orderDetail = new OrderItem
                    {
                        OrderId = newOrder.OrderId, // Lấy ID vừa sinh ở bước trên
                        FoodItemId = item.FoodItemId,
                        FoodName = item.Name,
                        FoodPrice = item.Price,
                        Quantity = 1
                    };
                    _context.OrderItems.Add(orderDetail);
                }

                _context.SaveChanges(); // Lưu lần 2: Lưu tất cả món ăn vào DB

                // Bước 4: Thông báo thành công
                MessageBox.Show($"Đặt hàng thành công!\nĐơn hàng #{newOrder.OrderId} đã được chuyển tới Shipper.", "Thành công");

                // Bước 5: Đặt kết quả trả về và ĐÓNG cửa sổ
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu đơn hàng: " + (ex.InnerException?.Message ?? ex.Message));
            }
        }
    }
}
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
        private User _user; // Biến private lưu user

        // Sửa Constructor nhận 2 tham số: Danh sách món và Người dùng
        public CheckoutWindow(List<FoodItem> items, User user)
        {
            InitializeComponent();
            _selectedItems = items;
            _user = user;

            lbSummary.ItemsSource = _selectedItems;
            txtTotal.Text = $"{_selectedItems.Sum(x => x.Price * (x.Quantity)):N0} VNĐ";

            // Tự động điền thông tin từ object User vào các ô nhập liệu
            txtFullName.Text = _user.FullName;
            txtEmail.Text = _user.Email;
            txtPhone.Text = _user.PhoneNumber;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ giao hàng!");
                return;
            }

            try
            {
                // Tạo đơn hàng mới sử dụng _user.UserId
                var newOrder = new Order
                {
                    CustomerId = _user.UserId,
                    RestaurantId = _selectedItems.First().RestaurantId,
                    TotalAmount = _selectedItems.Sum(x => x.Price * (x.Quantity)),
                    DeliveryAddress = txtAddress.Text,
                    Status = "WaitingShipper",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    PaymentMethod = "Cash",
                    Note = $"SĐT: {txtPhone.Text}"
                };

                _context.Orders.Add(newOrder);
                _context.SaveChanges();

                foreach (var item in _selectedItems)
                {
                    var orderDetail = new OrderItem
                    {
                        OrderId = newOrder.OrderId,
                        FoodItemId = item.FoodItemId,
                        FoodName = item.Name,
                        FoodPrice = item.Price,
                        Quantity = item.Quantity
                    };
                    _context.OrderItems.Add(orderDetail);
                }

                _context.SaveChanges();

                MessageBox.Show($"Đặt hàng thành công! Mã đơn: #{newOrder.OrderId}", "Thành công");
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu đơn hàng: " + ex.Message);
            }
        }
    }
}
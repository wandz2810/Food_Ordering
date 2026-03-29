using Food_Ordering.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Food_Ordering
{
    public partial class MainWindow : Window
    {
        private readonly FoodOrderingDbContext _context = new FoodOrderingDbContext();
        private ObservableCollection<FoodItem> _myCart = new ObservableCollection<FoodItem>();
        private User _currentUser;

        // Constructor duy nhất nhận User từ Login
        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            // Gán Source cho DataGrid ngay từ đầu
            dgCart.ItemsSource = _myCart;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lấy danh sách nhà hàng đang mở
                var openRestaurants = _context.Restaurants.Where(r => r.IsOpen == true).ToList();
                cbRestaurants.ItemsSource = openRestaurants;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách nhà hàng: " + ex.Message, "Lỗi hệ thống");
            }
        }

        private void CbRestaurants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRestaurants.SelectedItem is Restaurant selectedRest)
            {
                // Load món ăn của nhà hàng được chọn
                lbFoodItems.ItemsSource = _context.FoodItems
                    .Where(f => f.RestaurantId == selectedRest.RestaurantId && f.IsAvailable == true)
                    .ToList();

                // Lưu ý quan trọng: Đổi nhà hàng là xóa sạch giỏ hàng cũ để tránh shipper đi 2 quán khác nhau
                _myCart.Clear();
                UpdateTotal();
            }
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (lbFoodItems.SelectedItem is FoodItem selected)
            {
                // Kiểm tra xem món này có trong giỏ chưa
                var itemInCart = _myCart.FirstOrDefault(x => x.FoodItemId == selected.FoodItemId);

                if (itemInCart != null)
                {
                    itemInCart.Quantity++; // Tăng số lượng nếu đã tồn tại
                }
                else
                {
                    selected.Quantity = 1; // Mặc định là 1 cho món mới
                    _myCart.Add(selected);
                }

                // Buộc DataGrid vẽ lại để hiện số lượng mới
                dgCart.Items.Refresh();
                UpdateTotal();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một món ăn trước!", "Nhắc nhở");
            }
        }

        private void BtnIncrease_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is FoodItem item)
            {
                item.Quantity++;
                dgCart.Items.Refresh();
                UpdateTotal();
            }
        }

        private void BtnDecrease_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is FoodItem item)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    _myCart.Remove(item); // Về 0 thì bay màu khỏi giỏ hàng
                }
                dgCart.Items.Refresh();
                UpdateTotal();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is FoodItem selected)
            {
                _myCart.Remove(selected);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            if (txtTotal != null)
            {
                // Tính tổng tiền dựa trên giá và số lượng
                decimal total = _myCart.Sum(x => x.Price * x.Quantity);
                txtTotal.Text = $"{total:N0} VNĐ";
            }
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_myCart.Count == 0)
            {
                MessageBox.Show("Giỏ hàng của bạn đang trống. Hãy chọn món trước khi thanh toán!", "Thông báo");
                return;
            }

            // Truyền cả List và object User sang Checkout
            List<FoodItem> cartList = _myCart.ToList();
            CheckoutWindow checkoutWin = new CheckoutWindow(cartList, _currentUser);
            checkoutWin.Owner = this;

            if (checkoutWin.ShowDialog() == true)
            {
                // Nếu thanh toán xong thì dọn sạch giỏ hàng
                _myCart.Clear();
                UpdateTotal();

                // Mở lịch sử đơn hàng tự động để khách xem
                BtnHistory_Click(null, null);
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            // Truyền User sang để HistoryWindow biết lọc đơn hàng của ai
            HistoryWindow historyWin = new HistoryWindow(_currentUser);
            historyWin.Owner = this;
            historyWin.ShowDialog();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Bạn có muốn đăng xuất?", "Xác nhận", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // Mở lại Login
                LoginWindow loginWin = new LoginWindow();
                loginWin.Show();

                // QUAN TRỌNG: Đóng chính nó (MainWindow)
                this.Close();
            }
        }
    }
}
using Food_Ordering.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Food_Ordering
{
    public partial class MainWindow : Window
    {
        // Khởi tạo Database Context và Giỏ hàng (ObservableCollection giúp UI tự cập nhật)
        private readonly FoodOrderingDbContext _context = new FoodOrderingDbContext();
        private ObservableCollection<FoodItem> _myCart = new ObservableCollection<FoodItem>();

        public MainWindow()
        {
            InitializeComponent();
            // Kết nối DataGrid với danh sách giỏ hàng
            dgCart.ItemsSource = _myCart;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                // Chỉ giữ lại việc load danh sách nhà hàng
                cbRestaurants.ItemsSource = _context.Restaurants.Where(r => r.IsOpen).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }

        private void CbRestaurants_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Khi chọn nhà hàng, hiển thị danh sách món ăn của nhà hàng đó
            if (cbRestaurants.SelectedItem is Restaurant selectedRest)
            {
                lbFoodItems.ItemsSource = _context.FoodItems
                    .Where(f => f.RestaurantId == selectedRest.RestaurantId && f.IsAvailable).ToList();

                // Lưu ý: Đổi nhà hàng sẽ xóa giỏ hàng cũ để tránh nhầm lẫn món giữa các quán
                _myCart.Clear();
                UpdateTotal();
            }
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            // Thêm món được chọn từ ListBox vào giỏ hàng
            if (lbFoodItems.SelectedItem is FoodItem selected)
            {
                _myCart.Add(selected);
                UpdateTotal();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một món ăn từ thực đơn!");
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            // Xóa món đang chọn khỏi DataGrid giỏ hàng
            if (dgCart.SelectedItem is FoodItem selected)
            {
                _myCart.Remove(selected);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            // Tính tổng tiền và cập nhật lên giao diện
            if (txtTotal != null)
            {
                txtTotal.Text = $"Tổng cộng: {_myCart.Sum(x => x.Price):N0} VNĐ";
            }
        }

        // --- ĐOẠN LOGIC CHUYỂN QUA MÀN HÌNH XÁC NHẬN ---
        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {

            if (App.CurrentUserId == 0)
            {
                MessageBox.Show("Bạn chưa đăng nhập! Vui lòng quay lại màn hình Login.");
                return;
            }

            if (_myCart.Count == 0)
            {
                MessageBox.Show("Giỏ hàng đang trống!");
                return;
            }

            // 1. Chuyển giỏ hàng thành List để truyền sang cửa sổ mới
            List<FoodItem> cartList = _myCart.ToList();

            // 2. Khởi tạo CheckoutWindow (Sử dụng Constructor nhận List món ăn)
            CheckoutWindow checkoutWin = new CheckoutWindow(cartList);

            // 3. Căn giữa cửa sổ con theo cửa sổ chính
            checkoutWin.Owner = this;
            checkoutWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            // 4. Mở dưới dạng Dialog (Chờ kết quả thanh toán)
            if (checkoutWin.ShowDialog() == true)
            {
                // Nếu thanh toán thành công (DialogResult = true), xóa sạch giỏ hàng
                _myCart.Clear();
                UpdateTotal();

                // Gợi ý: Tự động mở lịch sử để khách theo dõi đơn hàng
                BtnHistory_Click(null, null);
            }
        }

        // --- MỞ MÀN HÌNH LỊCH SỬ ĐƠN HÀNG ---
        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            HistoryWindow historyWin = new HistoryWindow();
            historyWin.Owner = this;
            historyWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            historyWin.ShowDialog();
        }
    }
}
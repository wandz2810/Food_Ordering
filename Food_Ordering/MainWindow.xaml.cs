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
        private User _currentUser; // Lưu trữ User đăng nhập

        // Thêm đoạn này vào
        public MainWindow()
        {
            InitializeComponent();
            // Gán tạm một User giả để App không bị crash khi chạy trực tiếp
            _currentUser = new User { UserId = 1, FullName = "Admin Test" };
            dgCart.ItemsSource = _myCart;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                cbRestaurants.ItemsSource = _context.Restaurants.Where(r => r.IsOpen == true).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu: " + ex.Message);
            }
        }

        private void CbRestaurants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRestaurants.SelectedItem is Restaurant selectedRest)
            {
                lbFoodItems.ItemsSource = _context.FoodItems
                    .Where(f => f.RestaurantId == selectedRest.RestaurantId && f.IsAvailable == true).ToList();
                _myCart.Clear();
                UpdateTotal();
            }
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (lbFoodItems.SelectedItem is FoodItem selected)
            {
                var itemInCart = _myCart.FirstOrDefault(x => x.FoodItemId == selected.FoodItemId);
                if (itemInCart != null)
                {
                    itemInCart.Quantity++;
                }
                else
                {
                    selected.Quantity = 1;
                    _myCart.Add(selected);
                }
                dgCart.Items.Refresh();
                UpdateTotal();
            }
        }

        private void BtnIncrease_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is FoodItem item)
            {
                item.Quantity++;
                dgCart.Items.Refresh();
                UpdateTotal();
            }
        }

        private void BtnDecrease_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is FoodItem item)
            {
                if (item.Quantity > 1) item.Quantity--;
                else _myCart.Remove(item);
                dgCart.Items.Refresh();
                UpdateTotal();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).DataContext is FoodItem selected)
            {
                _myCart.Remove(selected);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            if (txtTotal != null)
            {
                decimal total = _myCart.Sum(x => x.Price * (x.Quantity));
                txtTotal.Text = $"{total:N0} VNĐ";
            }
        }

        private void BtnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_myCart.Count == 0)
            {
                MessageBox.Show("Giỏ hàng của bạn đang trống!");
                return;
            }

            // TRUYỀN cả giỏ hàng VÀ đối tượng User sang Checkout
            List<FoodItem> cartList = _myCart.ToList();
            CheckoutWindow checkoutWin = new CheckoutWindow(cartList, _currentUser);
            checkoutWin.Owner = this;

            if (checkoutWin.ShowDialog() == true)
            {
                _myCart.Clear();
                UpdateTotal();
                BtnHistory_Click(null, null);
            }
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            // TRUYỀN đối tượng User sang HistoryWindow
            HistoryWindow historyWin = new HistoryWindow(_currentUser);
            historyWin.Owner = this;
            historyWin.ShowDialog();
        }
    }
}
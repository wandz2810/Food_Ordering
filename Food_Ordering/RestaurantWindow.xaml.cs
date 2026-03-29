using System;
using System.Windows;
using System.Windows.Controls;
using Food_Ordering.Entities;

namespace Food_Ordering
{
    public partial class RestaurantWindow : Window
    {
        private readonly RestaurantOwnerService service;
        private int currentRestaurantId = 0; // resolved từ OwnerId
        private User _currentUser; // có thể dùng để hiển thị thông tin user hoặc kiểm tra quyền

        public RestaurantWindow(User user)
        {
            InitializeComponent();
            service = new RestaurantOwnerService(user);

            // Tự động thử load nhà hàng của user hiện tại (nếu App.CurrentUserId đã được set)
            InitializeOwnerContext();
            _currentUser = user;
        }

        // Tìm và khởi tạo context nhà hàng theo ownerId (nếu ownerId == null thì dùng App.CurrentUserId)
        private void InitializeOwnerContext(int? ownerId = null)
        {
            // FIX: dùng null-coalescing để chuyển int? => int (sử dụng App.CurrentUserId khi ownerId == null)
            int id = ownerId ?? App.CurrentUserId;
            if (id <= 0)
            {
                DisableOwnerControls();
                return;
            }

            var rest = service.GetRestaurant(id);
            if (rest == null)
            {
                currentRestaurantId = 0;
                DisableOwnerControls();
                MessageBox.Show($"Không tìm thấy nhà hàng cho OwnerId = {id}.", "Không tìm thấy", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            currentRestaurantId = rest.RestaurantId;
            EnableOwnerControls();
            LoadMenu();
        }

        private void EnableOwnerControls()
        {
            btnAdd.IsEnabled = btnUpdate.IsEnabled = btnDelete.IsEnabled = btnSoldOut.IsEnabled = btnRestore.IsEnabled = true;
            dgMenu.IsEnabled = true;
        }

        private void DisableOwnerControls()
        {
            btnAdd.IsEnabled = btnUpdate.IsEnabled = btnDelete.IsEnabled = btnSoldOut.IsEnabled = btnRestore.IsEnabled = false;
            dgMenu.IsEnabled = false;
            ClearInputs();
        }

        private void LoadMenu()
        {
            if (currentRestaurantId <= 0)
            {
                dgMenu.ItemsSource = null;
                return;
            }
            dgMenu.ItemsSource = service.GetMenu(currentRestaurantId);
        }

        private void ClearInputs()
        {
            txtName.Text = string.Empty;
            txtPrice.Text = string.Empty;
        }

        // nút tìm bằng OwnerId thủ công
        private void btnFindOwner_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtOwnerIdSearch.Text, out var ownerId) || ownerId <= 0)
            {
                MessageBox.Show("Vui lòng nhập OwnerId hợp lệ.", "Kiểm tra", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            InitializeOwnerContext(ownerId);
        }

        // nút tải nhà hàng gắn với user hiện tại
        private void btnLoadMine_Click(object sender, RoutedEventArgs e)
        {
            InitializeOwnerContext(null);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (currentRestaurantId <= 0)
            {
                MessageBox.Show("Không có nhà hàng để thêm món.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text) || !decimal.TryParse(txtPrice.Text, out var price) || price <= 0)
            {
                MessageBox.Show("Tên món không được để trống và giá phải > 0.", "Kiểm tra", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var food = new FoodItem
            {
                Name = txtName.Text.Trim(),
                Price = price,
                RestaurantId = currentRestaurantId,
                IsAvailable = true
            };

            try
            {
                service.AddFood(food);
                LoadMenu();
                ClearInputs();
                MessageBox.Show("Thêm món thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm món: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgMenu.SelectedItem is not FoodItem selected)
            {
                MessageBox.Show("Vui lòng chọn món để cập nhật.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text) || !decimal.TryParse(txtPrice.Text, out var price) || price <= 0)
            {
                MessageBox.Show("Tên món không được để trống và giá phải > 0.", "Kiểm tra", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            selected.Name = txtName.Text.Trim();
            selected.Price = price;

            try
            {
                service.UpdateFood(selected);
                LoadMenu();
                ClearInputs();
                MessageBox.Show("Cập nhật món thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgMenu.SelectedItem is not FoodItem selected)
            {
                MessageBox.Show("Vui lòng chọn món để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa '{selected.Name}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                service.DeleteFood(selected.FoodItemId);
                LoadMenu();
                ClearInputs();
                MessageBox.Show("Xóa món thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi xóa: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSoldOut_Click(object sender, RoutedEventArgs e)
        {
            if (dgMenu.SelectedItem is not FoodItem selected)
            {
                MessageBox.Show("Vui lòng chọn món để đánh dấu.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                service.MarkSoldOutForToday(selected.FoodItemId);
                LoadMenu();
                MessageBox.Show($"Đã đánh dấu '{selected.Name}' là hết trong ngày.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (dgMenu.SelectedItem is not FoodItem selected)
            {
                MessageBox.Show("Vui lòng chọn món để mở lại.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                service.RestoreAvailability(selected.FoodItemId);
                LoadMenu();
                MessageBox.Show($"Đã mở lại '{selected.Name}'.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMenu.SelectedItem is FoodItem selected)
            {
                txtName.Text = selected.Name;
                txtPrice.Text = selected.Price.ToString();
            }
            else
            {
                ClearInputs();
            }
        }
    }
}
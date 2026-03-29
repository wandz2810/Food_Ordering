using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Food_Ordering.Entities;
using Food_Ordering.Service;

namespace Food_Ordering.Admin
{
    public partial class AdminWindow : Window
    {
        private readonly AdminService _service;
        private readonly User _currentUser;

        public AdminWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _service = new AdminService();
            InitializeComboBoxes();
            LoadDashboard();
        }

        private void InitializeComboBoxes()
        {
            cbUserRole.ItemsSource = new List<string> { "Customer", "Shipper", "RestaurantOwner" };
            cbUserRole.SelectedIndex = 0;

            cbVehicleType.ItemsSource = new List<string> { "motorbike", "bicycle", "car" };
            cbVehicleType.SelectedIndex = 0;

            cbUserFilter.ItemsSource = new List<string> { "All", "Customer", "Shipper", "RestaurantOwner", "Admin" };
            cbUserFilter.SelectedIndex = 0;

            cbEditStatus.ItemsSource = new List<string> { "Active", "Inactive", "Banned" };
            cbEditStatus.SelectedIndex = 0;

            cbOrderStatusFilter.ItemsSource = new List<string> { "All", "PendingPayment", "PaymentSuccess", "PaymentFailed", "WaitingShipper", "HeadingToRestaurant", "WaitingFood", "Delivering", "Delivered", "Cancelled" };
            cbOrderStatusFilter.SelectedIndex = 0;

            cbOrderNewStatus.ItemsSource = new List<string> { "PendingPayment", "PaymentSuccess", "PaymentFailed", "WaitingShipper", "HeadingToRestaurant", "WaitingFood", "Delivering", "Delivered", "Cancelled" };
            cbOrderNewStatus.SelectedIndex = 0;

            cbShipperAppFilter.ItemsSource = new List<string> { "All", "Pending", "Approved", "Rejected" };
            cbShipperAppFilter.SelectedIndex = 0;

            cbRestAppFilter.ItemsSource = new List<string> { "All", "Pending", "Approved", "Rejected" };
            cbRestAppFilter.SelectedIndex = 0;
        }

        // ═══════════════════════════════════════════
        //  TAB NAVIGATION
        // ═══════════════════════════════════════════

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabMain == null) return;
            switch (tabMain.SelectedIndex)
            {
                case 0: LoadDashboard(); break;
                case 1: LoadUsers(); break;
                case 2: LoadRestaurants(); break;
                case 3: LoadOrders(); break;
                case 4: LoadFoodItems(); break;
                case 5: LoadApplications(); break;
            }
        }

        // ═══════════════════════════════════════════
        //  DASHBOARD
        // ═══════════════════════════════════════════

        private void LoadDashboard()
        {
            try
            {
                var stats = new List<DashboardRow>
                {
                    new DashboardRow { Metric = "Total Users", Value = _service.GetUserCount().ToString() },
                    new DashboardRow { Metric = "Customers", Value = _service.GetCustomerCount().ToString() },
                    new DashboardRow { Metric = "Shippers", Value = _service.GetShipperCount().ToString() },
                    new DashboardRow { Metric = "Restaurant Owners", Value = _service.GetRestaurantOwnerCount().ToString() },
                    new DashboardRow { Metric = "Restaurants", Value = _service.GetRestaurantCount().ToString() },
                    new DashboardRow { Metric = "Total Orders", Value = _service.GetOrderCount().ToString() },
                    new DashboardRow { Metric = "Pending Orders", Value = _service.GetPendingOrderCount().ToString() },
                    new DashboardRow { Metric = "Pending Applications", Value = _service.GetPendingApplicationCount().ToString() },
                    new DashboardRow { Metric = "Total Revenue (Delivered)", Value = _service.GetTotalRevenue().ToString("N0") + " VND" },
                };
                dgDashboard.ItemsSource = stats;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════
        //  USERS
        // ═══════════════════════════════════════════

        private void LoadUsers()
        {
            try
            {
                var keyword = txtUserSearch?.Text ?? "";
                var role = cbUserFilter?.SelectedItem as string;
                dgUsers.ItemsSource = _service.SearchUsers(keyword, role);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbUserRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbUserRole.SelectedItem == null) return;
            var role = cbUserRole.SelectedItem as string;

            var showCustomer = role == "Customer" ? Visibility.Visible : Visibility.Collapsed;
            var showShipper = role == "Shipper" ? Visibility.Visible : Visibility.Collapsed;

            if (lblAddress != null) lblAddress.Visibility = showCustomer;
            if (txtUserAddress != null) txtUserAddress.Visibility = showCustomer;
            if (lblVehicle != null) lblVehicle.Visibility = showShipper;
            if (cbVehicleType != null) cbVehicleType.Visibility = showShipper;
            if (lblPlate != null) lblPlate.Visibility = showShipper;
            if (txtUserLicensePlate != null) txtUserLicensePlate.Visibility = showShipper;
        }

        private void btnCreateUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUserEmail.Text) ||
                    string.IsNullOrWhiteSpace(txtUserPassword.Password) ||
                    string.IsNullOrWhiteSpace(txtUserFullName.Text) ||
                    string.IsNullOrWhiteSpace(txtUserPhone.Text))
                {
                    MessageBox.Show("Please fill in all required fields.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var role = cbUserRole.SelectedItem as string ?? "Customer";
                var user = new User
                {
                    Email = txtUserEmail.Text.Trim(),
                    PasswordHash = txtUserPassword.Password,
                    FullName = txtUserFullName.Text.Trim(),
                    PhoneNumber = txtUserPhone.Text.Trim()
                };

                string? vehicleType = role == "Shipper" ? (cbVehicleType.SelectedItem as string ?? "motorbike") : null;
                string? licensePlate = role == "Shipper" ? txtUserLicensePlate.Text.Trim() : null;

                _service.CreateUser(user, role, txtUserAddress.Text.Trim(), vehicleType, licensePlate);
                MessageBox.Show("User created!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                txtUserEmail.Clear(); txtUserPassword.Clear(); txtUserFullName.Clear();
                txtUserPhone.Clear(); txtUserAddress.Clear(); txtUserLicensePlate.Clear();
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers.SelectedItem is User selected)
            {
                txtEditFullName.Text = selected.FullName;
                txtEditPhone.Text = selected.PhoneNumber;
                cbEditStatus.SelectedItem = selected.Status;
            }
        }

        private void btnUpdateUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is not User selected)
            {
                MessageBox.Show("Please select a user.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                _service.UpdateUser(selected.UserId, txtEditFullName.Text.Trim(),
                    txtEditPhone.Text.Trim(), cbEditStatus.SelectedItem as string ?? "Active");
                MessageBox.Show("User updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is not User selected)
            {
                MessageBox.Show("Please select a user.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (selected.Role == "Admin")
            {
                MessageBox.Show("Cannot delete admin.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"Delete '{selected.FullName}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            try
            {
                _service.DeleteUser(selected.UserId);
                MessageBox.Show("User deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtUserSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadUsers();
        private void cbUserFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadUsers();

        // ═══════════════════════════════════════════
        //  RESTAURANTS
        // ═══════════════════════════════════════════

        private void LoadRestaurants()
        {
            try
            {
                dgRestaurants.ItemsSource = _service.GetAllRestaurants();

                var owners = _service.GetAvailableOwners().Select(o => new OwnerComboItem
                {
                    OwnerId = o.OwnerId,
                    DisplayText = $"{o.Owner.FullName} (ID: {o.OwnerId})"
                }).ToList();
                cbRestOwner.ItemsSource = owners;
                if (owners.Any()) cbRestOwner.SelectedIndex = 0;

                LoadFoodRestaurantFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCreateRestaurant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbRestOwner.SelectedItem is not OwnerComboItem selectedOwner)
                {
                    MessageBox.Show("Select an owner.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtRestName.Text) || string.IsNullOrWhiteSpace(txtRestAddress.Text))
                {
                    MessageBox.Show("Fill in name and address.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                _service.CreateRestaurant(new Restaurant
                {
                    OwnerId = selectedOwner.OwnerId,
                    Name = txtRestName.Text.Trim(),
                    Address = txtRestAddress.Text.Trim(),
                    Description = txtRestDesc.Text.Trim()
                });
                MessageBox.Show("Restaurant created!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                txtRestName.Clear(); txtRestAddress.Clear(); txtRestDesc.Clear();
                LoadRestaurants();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgRestaurants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRestaurants.SelectedItem is Restaurant r)
            {
                txtEditRestName.Text = r.Name;
                txtEditRestAddress.Text = r.Address;
                txtEditRestDesc.Text = r.Description;
                chkEditRestOpen.IsChecked = r.IsOpen;
            }
        }

        private void btnUpdateRestaurant_Click(object sender, RoutedEventArgs e)
        {
            if (dgRestaurants.SelectedItem is not Restaurant selected)
            {
                MessageBox.Show("Select a restaurant.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                selected.Name = txtEditRestName.Text.Trim();
                selected.Address = txtEditRestAddress.Text.Trim();
                selected.Description = txtEditRestDesc.Text.Trim();
                selected.IsOpen = chkEditRestOpen.IsChecked ?? true;
                _service.UpdateRestaurant(selected);
                MessageBox.Show("Updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRestaurants();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDeleteRestaurant_Click(object sender, RoutedEventArgs e)
        {
            if (dgRestaurants.SelectedItem is not Restaurant selected)
            {
                MessageBox.Show("Select a restaurant.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (MessageBox.Show($"Delete '{selected.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;
            try
            {
                _service.DeleteRestaurant(selected.RestaurantId);
                MessageBox.Show("Deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRestaurants();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════
        //  ORDERS
        // ═══════════════════════════════════════════

        private void LoadOrders()
        {
            try
            {
                dgOrders.ItemsSource = _service.SearchOrders(cbOrderStatusFilter?.SelectedItem as string);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbOrderStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadOrders();

        private void dgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order o)
            {
                lblSelectedOrderId.Text = o.OrderId.ToString();
                cbOrderNewStatus.SelectedItem = o.Status;
                dgOrderItems.ItemsSource = o.OrderItems;
            }
            else
            {
                lblSelectedOrderId.Text = "-";
                dgOrderItems.ItemsSource = null;
            }
        }

        private void btnUpdateOrderStatus_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem is not Order selected)
            {
                MessageBox.Show("Select an order.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                _service.UpdateOrderStatus(selected.OrderId, cbOrderNewStatus.SelectedItem as string ?? "PendingPayment");
                MessageBox.Show("Status updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadOrders();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════
        //  FOOD ITEMS
        // ═══════════════════════════════════════════

        private void LoadFoodItems()
        {
            try
            {
                LoadFoodRestaurantFilter();
                if (cbFoodRestFilter?.SelectedItem is RestaurantComboItem r && r.RestaurantId > 0)
                    dgFoodItems.ItemsSource = _service.GetFoodItemsByRestaurant(r.RestaurantId);
                else
                    dgFoodItems.ItemsSource = _service.GetAllFoodItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFoodRestaurantFilter()
        {
            if (cbFoodRestFilter == null) return;
            var current = cbFoodRestFilter.SelectedItem;
            var items = new List<RestaurantComboItem> { new() { RestaurantId = 0, DisplayText = "All Restaurants" } };
            items.AddRange(_service.GetAllRestaurants().Select(r => new RestaurantComboItem { RestaurantId = r.RestaurantId, DisplayText = r.Name }));
            cbFoodRestFilter.DisplayMemberPath = "DisplayText";
            cbFoodRestFilter.ItemsSource = items;
            if (current == null) cbFoodRestFilter.SelectedIndex = 0;
        }

        private void cbFoodRestFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFoodRestFilter?.SelectedItem is RestaurantComboItem r)
            {
                try
                {
                    dgFoodItems.ItemsSource = r.RestaurantId > 0
                        ? _service.GetFoodItemsByRestaurant(r.RestaurantId)
                        : _service.GetAllFoodItems();
                }
                catch { }
            }
        }

        private void btnToggleAvailability_Click(object sender, RoutedEventArgs e)
        {
            if (dgFoodItems.SelectedItem is not FoodItem selected)
            {
                MessageBox.Show("Select a food item.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try { _service.ToggleFoodAvailability(selected.FoodItemId); LoadFoodItems(); }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        // ═══════════════════════════════════════════
        //  APPLICATIONS
        // ═══════════════════════════════════════════

        private void LoadApplications()
        {
            try
            {
                dgShipperApps.ItemsSource = _service.GetShipperApplications(cbShipperAppFilter?.SelectedItem as string);
                dgRestApps.ItemsSource = _service.GetRestaurantApplications(cbRestAppFilter?.SelectedItem as string);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cbShipperAppFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadApplications();
        private void cbRestAppFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadApplications();
        private void dgShipperApps_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void dgRestApps_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void btnApproveShipperApp_Click(object sender, RoutedEventArgs e)
        {
            if (dgShipperApps.SelectedItem is not ShipperApplication app || app.Status != "Pending")
            {
                MessageBox.Show("Select a pending application.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                _service.ApproveShipperApplication(app.ApplicationId, txtShipperAppNote.Text.Trim());
                MessageBox.Show("Approved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                txtShipperAppNote.Clear(); LoadApplications();
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void btnRejectShipperApp_Click(object sender, RoutedEventArgs e)
        {
            if (dgShipperApps.SelectedItem is not ShipperApplication app || app.Status != "Pending")
            {
                MessageBox.Show("Select a pending application.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                _service.RejectShipperApplication(app.ApplicationId, txtShipperAppNote.Text.Trim());
                MessageBox.Show("Rejected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                txtShipperAppNote.Clear(); LoadApplications();
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void btnApproveRestApp_Click(object sender, RoutedEventArgs e)
        {
            if (dgRestApps.SelectedItem is not RestaurantApplication app || app.Status != "Pending")
            {
                MessageBox.Show("Select a pending application.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                _service.ApproveRestaurantApplication(app.ApplicationId, txtRestAppNote.Text.Trim());
                MessageBox.Show("Approved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                txtRestAppNote.Clear(); LoadApplications();
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void btnRejectRestApp_Click(object sender, RoutedEventArgs e)
        {
            if (dgRestApps.SelectedItem is not RestaurantApplication app || app.Status != "Pending")
            {
                MessageBox.Show("Select a pending application.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            try
            {
                _service.RejectRestaurantApplication(app.ApplicationId, txtRestAppNote.Text.Trim());
                MessageBox.Show("Rejected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                txtRestAppNote.Clear(); LoadApplications();
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        // ═══════════════════════════════════════════
        //  LOGOUT
        // ═══════════════════════════════════════════

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Logout?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                new LoginWindow().Show();
                this.Close();
            }
        }
    }

    // Helper classes
    public class DashboardRow { public string Metric { get; set; } = ""; public string Value { get; set; } = ""; }
    public class OwnerComboItem { public int OwnerId { get; set; } public string DisplayText { get; set; } = ""; }
    public class RestaurantComboItem { public int RestaurantId { get; set; } public string DisplayText { get; set; } = ""; }
}

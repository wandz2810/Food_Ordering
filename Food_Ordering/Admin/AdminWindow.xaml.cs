using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Food_Ordering.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Food_Ordering.Admin
{
    public partial class AdminWindow : Window
    {
        private readonly FoodOrderingDbContext _db;
        private readonly User _currentUser;

        public AdminWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _db = new FoodOrderingDbContext();
            InitializeComboBoxes();
            LoadDashboard();
        }

        private void InitializeComboBoxes()
        {
            cbUserRole.ItemsSource     = new List<string> { "Customer", "Shipper", "RestaurantOwner" };
            cbUserRole.SelectedIndex   = 0;
            cbVehicleType.ItemsSource  = new List<string> { "motorbike", "bicycle", "car" };
            cbVehicleType.SelectedIndex = 0;
            cbUserFilter.ItemsSource   = new List<string> { "All", "Customer", "Shipper", "RestaurantOwner", "Admin" };
            cbUserFilter.SelectedIndex = 0;
            cbEditStatus.ItemsSource   = new List<string> { "Active", "Inactive", "Banned" };
            cbEditStatus.SelectedIndex = 0;
            cbOrderStatusFilter.ItemsSource = new List<string> { "All", "PendingPayment", "PaymentSuccess", "PaymentFailed", "Đang chờ tài xế", "HeadingToRestaurant", "WaitingFood", "Delivering", "Delivered", "Cancelled" };
            cbOrderStatusFilter.SelectedIndex = 0;
            cbOrderNewStatus.ItemsSource = new List<string> { "PendingPayment", "PaymentSuccess", "PaymentFailed", "Đang chờ tài xế", "HeadingToRestaurant", "WaitingFood", "Delivering", "Delivered", "Cancelled" };
            cbOrderNewStatus.SelectedIndex = 0;
        }

        // ═══════════════════════════════════════════
        //  TAB NAVIGATION
        // ═══════════════════════════════════════════

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard: SelectionChanged bubbles up from nested ComboBoxes — only handle the TabControl itself
            if (e.OriginalSource != tabMain || tabMain == null) return;
            switch (tabMain.SelectedIndex)
            {
                case 0: LoadDashboard();   break;
                case 1: LoadUsers();       break;
                case 2: LoadRestaurants(); break;
                case 3: LoadOrders();      break;
                case 4: LoadFoodItems();   break;
            }
        }

        // ═══════════════════════════════════════════
        //  DASHBOARD
        // ═══════════════════════════════════════════

        private void LoadDashboard()
        {
            try
            {
                var pendingStatuses = new[] { "PendingPayment", "Đang chờ tài xế", "WaitingFood" };
                var stats = new List<DashboardRow>
                {
                    new() { Metric = "Total Users",              Value = _db.Users.Count().ToString() },
                    new() { Metric = "Customers",                Value = _db.Customers.Count().ToString() },
                    new() { Metric = "Shippers",                 Value = _db.Shippers.Count().ToString() },
                    new() { Metric = "Restaurant Owners",        Value = _db.RestaurantOwners.Count().ToString() },
                    new() { Metric = "Restaurants",              Value = _db.Restaurants.Count().ToString() },
                    new() { Metric = "Total Orders",             Value = _db.Orders.Count().ToString() },
                    new() { Metric = "Pending Orders",           Value = _db.Orders.Count(o => pendingStatuses.Contains(o.Status)).ToString() },
                    new() { Metric = "Total Revenue (Delivered)",Value = (_db.Orders.Where(o => o.Status == "Delivered").Sum(o => (decimal?)o.TotalAmount) ?? 0).ToString("N0") + " VND" },
                };
                dgDashboard.ItemsSource = stats;
            }
            catch (Exception ex) { ShowError(ex); }
        }

        // ═══════════════════════════════════════════
        //  USERS
        // ═══════════════════════════════════════════

        private void LoadUsers()
        {
            try
            {
                var keyword = txtUserSearch?.Text?.ToLower() ?? "";
                var role    = cbUserFilter?.SelectedItem as string;

                var query = _db.Users.AsQueryable();
                if (!string.IsNullOrWhiteSpace(keyword))
                    query = query.Where(u => u.Email.ToLower().Contains(keyword) ||
                                             u.FullName.ToLower().Contains(keyword) ||
                                             u.PhoneNumber.Contains(keyword));
                if (!string.IsNullOrWhiteSpace(role) && role != "All")
                    query = query.Where(u => u.Role == role);

                dgUsers.ItemsSource = query.OrderByDescending(u => u.CreatedAt).ToList();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void cbUserRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var role       = cbUserRole.SelectedItem as string;
            var showCustomer = role == "Customer" ? Visibility.Visible : Visibility.Collapsed;
            var showShipper  = role == "Shipper"  ? Visibility.Visible : Visibility.Collapsed;
            if (lblAddress != null)         lblAddress.Visibility         = showCustomer;
            if (txtUserAddress != null)     txtUserAddress.Visibility     = showCustomer;
            if (lblVehicle != null)         lblVehicle.Visibility         = showShipper;
            if (cbVehicleType != null)      cbVehicleType.Visibility      = showShipper;
            if (lblPlate != null)           lblPlate.Visibility           = showShipper;
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
                if (_db.Users.Any(u => u.Email.ToLower() == txtUserEmail.Text.ToLower()))
                    throw new InvalidOperationException("Email already exists.");

                var user = new User
                {
                    Email        = txtUserEmail.Text.Trim(),
                    PasswordHash = txtUserPassword.Password,
                    FullName     = txtUserFullName.Text.Trim(),
                    PhoneNumber  = txtUserPhone.Text.Trim(),
                    Role         = role,
                    Status       = "Active",
                    CreatedAt    = DateTime.Now
                };
                _db.Users.Add(user);
                _db.SaveChanges();

                switch (role)
                {
                    case "Customer":
                        _db.Customers.Add(new Customer { CustomerId = user.UserId, Address = txtUserAddress.Text.Trim() });
                        break;
                    case "Shipper":
                        _db.Shippers.Add(new Entities.Shipper
                        {
                            ShipperId   = user.UserId,
                            VehicleType = cbVehicleType.SelectedItem as string ?? "motorbike",
                            LicensePlate = txtUserLicensePlate.Text.Trim(),
                            IsAvailable = true
                        });
                        break;
                    case "RestaurantOwner":
                        _db.RestaurantOwners.Add(new RestaurantOwner { OwnerId = user.UserId });
                        break;
                }
                _db.SaveChanges();

                MessageBox.Show("User created!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                txtUserEmail.Clear(); txtUserPassword.Clear(); txtUserFullName.Clear();
                txtUserPhone.Clear(); txtUserAddress.Clear(); txtUserLicensePlate.Clear();
                LoadUsers();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUsers.SelectedItem is User u)
            {
                txtEditFullName.Text      = u.FullName;
                txtEditPhone.Text         = u.PhoneNumber;
                cbEditStatus.SelectedItem = u.Status;
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
                var user = _db.Users.Find(selected.UserId) ?? throw new InvalidOperationException("User not found.");
                user.FullName    = txtEditFullName.Text.Trim();
                user.PhoneNumber = txtEditPhone.Text.Trim();
                user.Status      = cbEditStatus.SelectedItem as string ?? "Active";
                _db.SaveChanges();
                MessageBox.Show("User updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is not User selected)
            {
                MessageBox.Show("Please select a user.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (selected.Role == "Admin") { MessageBox.Show("Cannot delete admin.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            if (MessageBox.Show($"Delete '{selected.FullName}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;

            try
            {
                var user = _db.Users
                    .Include(u => u.Customer)
                    .Include(u => u.Shipper)
                    .Include(u => u.RestaurantOwner)
                    .FirstOrDefault(u => u.UserId == selected.UserId)
                    ?? throw new InvalidOperationException("User not found.");

                if (user.Customer != null)
                {
                    var cart = _db.Carts.FirstOrDefault(c => c.CustomerId == user.UserId);
                    if (cart != null) { _db.CartItems.RemoveRange(_db.CartItems.Where(ci => ci.CartId == cart.CartId)); _db.Carts.Remove(cart); }
                    _db.Customers.Remove(user.Customer);
                }
                if (user.Shipper != null) _db.Shippers.Remove(user.Shipper);
                if (user.RestaurantOwner != null)
                {
                    var rest = _db.Restaurants.FirstOrDefault(r => r.OwnerId == user.UserId);
                    if (rest != null) { _db.FoodItems.RemoveRange(_db.FoodItems.Where(f => f.RestaurantId == rest.RestaurantId)); _db.Restaurants.Remove(rest); }
                    _db.RestaurantOwners.Remove(user.RestaurantOwner);
                }
                _db.ShipperApplications.RemoveRange(_db.ShipperApplications.Where(a => a.UserId == user.UserId));
                _db.RestaurantApplications.RemoveRange(_db.RestaurantApplications.Where(a => a.UserId == user.UserId));
                _db.Users.Remove(user);
                _db.SaveChanges();

                MessageBox.Show("User deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadUsers();
            }
            catch (Exception ex) { ShowError(ex); }
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
                dgRestaurants.ItemsSource = GetAllRestaurants();

                var owners = GetAllOwners().Select(o => new OwnerComboItem
                {
                    OwnerId     = o.OwnerId,
                    DisplayText = $"{o.FullName} (ID: {o.OwnerId})"
                }).ToList();
                cbRestOwner.ItemsSource = owners;
                if (owners.Any()) cbRestOwner.SelectedIndex = 0;

                LoadFoodRestaurantFilter();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        // Raw SQL — avoids EF circular navigation between Restaurant ↔ RestaurantOwner
        private List<RestaurantDto> GetAllRestaurants()
        {
            var results = new List<RestaurantDto>();
            using var conn = new SqlConnection(_db.Database.GetConnectionString());
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT r.RestaurantId, r.Name, r.Address, r.IsOpen,
                       r.AverageRating, r.TotalOrders, r.Description,
                       r.LogoUrl, r.OwnerId, r.CreatedAt,
                       ISNULL(u.FullName, '') AS OwnerName
                FROM Restaurants r
                LEFT JOIN RestaurantOwners ro ON r.OwnerId = ro.OwnerId
                LEFT JOIN Users u ON ro.OwnerId = u.UserId
                ORDER BY r.CreatedAt DESC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                results.Add(new RestaurantDto
                {
                    RestaurantId  = reader.GetInt32(0),
                    Name          = reader.GetString(1),
                    Address       = reader.GetString(2),
                    IsOpen        = reader.GetBoolean(3),
                    AverageRating = reader.GetDecimal(4),
                    TotalOrders   = reader.GetInt32(5),
                    Description   = reader.IsDBNull(6) ? null : reader.GetString(6),
                    LogoUrl       = reader.IsDBNull(7) ? null : reader.GetString(7),
                    OwnerId       = reader.GetInt32(8),
                    CreatedAt     = reader.GetDateTime(9),
                    OwnerName     = reader.GetString(10)
                });
            return results;
        }

        // Raw SQL — avoids EF circular navigation; labels owners who already have a restaurant
        private List<OwnerDto> GetAllOwners()
        {
            var results = new List<OwnerDto>();
            using var conn = new SqlConnection(_db.Database.GetConnectionString());
            conn.Open();
            using var cmd = new SqlCommand(@"
                SELECT ro.OwnerId,
                       ISNULL(u.FullName, '') AS FullName,
                       CASE WHEN r.RestaurantId IS NOT NULL THEN 1 ELSE 0 END AS HasRestaurant
                FROM RestaurantOwners ro
                LEFT JOIN Users u ON ro.OwnerId = u.UserId
                LEFT JOIN Restaurants r ON r.OwnerId = ro.OwnerId
                ORDER BY HasRestaurant, u.FullName", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bool has = reader.GetInt32(2) == 1;
                results.Add(new OwnerDto
                {
                    OwnerId  = reader.GetInt32(0),
                    FullName = reader.GetString(1) + (has ? " (has restaurant)" : "")
                });
            }
            return results;
        }

        private void btnCreateRestaurant_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbRestOwner.SelectedItem is not OwnerComboItem owner) { MessageBox.Show("Select an owner.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                if (string.IsNullOrWhiteSpace(txtRestName.Text) || string.IsNullOrWhiteSpace(txtRestAddress.Text)) { MessageBox.Show("Fill in name and address.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
                if (_db.Restaurants.Any(r => r.OwnerId == owner.OwnerId)) throw new InvalidOperationException("This owner already has a restaurant.");

                _db.Restaurants.Add(new Restaurant
                {
                    OwnerId       = owner.OwnerId,
                    Name          = txtRestName.Text.Trim(),
                    Address       = txtRestAddress.Text.Trim(),
                    Description   = txtRestDesc.Text.Trim(),
                    IsOpen        = true,
                    AverageRating = 0,
                    TotalOrders   = 0,
                    CreatedAt     = DateTime.Now
                });
                _db.SaveChanges();
                MessageBox.Show("Restaurant created!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                txtRestName.Clear(); txtRestAddress.Clear(); txtRestDesc.Clear();
                LoadRestaurants();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void dgRestaurants_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgRestaurants.SelectedItem is RestaurantDto r)
            {
                txtEditRestName.Text    = r.Name;
                txtEditRestAddress.Text = r.Address;
                txtEditRestDesc.Text    = r.Description;
                chkEditRestOpen.IsChecked = r.IsOpen;
            }
        }

        private void btnUpdateRestaurant_Click(object sender, RoutedEventArgs e)
        {
            if (dgRestaurants.SelectedItem is not RestaurantDto selected) { MessageBox.Show("Select a restaurant.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            try
            {
                var rest = _db.Restaurants.Find(selected.RestaurantId) ?? throw new InvalidOperationException("Restaurant not found.");
                rest.Name        = txtEditRestName.Text.Trim();
                rest.Address     = txtEditRestAddress.Text.Trim();
                rest.Description = txtEditRestDesc.Text.Trim();
                rest.IsOpen      = chkEditRestOpen.IsChecked ?? true;
                _db.SaveChanges();
                MessageBox.Show("Updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRestaurants();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void btnDeleteRestaurant_Click(object sender, RoutedEventArgs e)
        {
            if (dgRestaurants.SelectedItem is not RestaurantDto selected) { MessageBox.Show("Select a restaurant.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            if (MessageBox.Show($"Delete '{selected.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            try
            {
                var rest = _db.Restaurants.Find(selected.RestaurantId) ?? throw new InvalidOperationException("Restaurant not found.");
                _db.FoodItems.RemoveRange(_db.FoodItems.Where(f => f.RestaurantId == rest.RestaurantId));
                _db.Restaurants.Remove(rest);
                _db.SaveChanges();
                MessageBox.Show("Deleted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRestaurants();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        // ═══════════════════════════════════════════
        //  ORDERS
        // ═══════════════════════════════════════════

        private void LoadOrders()
        {
            try
            {
                var statusFilter = cbOrderStatusFilter?.SelectedItem as string;
                var query = _db.Orders
                    .Include(o => o.Customer).ThenInclude(c => c.CustomerNavigation)
                    .Include(o => o.Restaurant)
                    .Include(o => o.Shipper).ThenInclude(s => s != null ? s.ShipperNavigation : null)
                    .Include(o => o.OrderItems)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                    query = query.Where(o => o.Status == statusFilter);

                dgOrders.ItemsSource = query.OrderByDescending(o => o.CreatedAt).ToList();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void cbOrderStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadOrders();

        private void dgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order o)
            {
                lblSelectedOrderId.Text    = o.OrderId.ToString();
                cbOrderNewStatus.SelectedItem = o.Status;
                dgOrderItems.ItemsSource   = o.OrderItems;
            }
            else { lblSelectedOrderId.Text = "-"; dgOrderItems.ItemsSource = null; }
        }

        private void btnUpdateOrderStatus_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem is not Order selected) { MessageBox.Show("Select an order.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            try
            {
                var order = _db.Orders.Find(selected.OrderId) ?? throw new InvalidOperationException("Order not found.");
                order.Status    = cbOrderNewStatus.SelectedItem as string ?? "PendingPayment";
                order.UpdatedAt = DateTime.Now;
                _db.SaveChanges();
                MessageBox.Show("Status updated!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadOrders();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        // ═══════════════════════════════════════════
        //  FOOD ITEMS
        // ═══════════════════════════════════════════

        private void LoadFoodItems()
        {
            try
            {
                LoadFoodRestaurantFilter();
                var filter = cbFoodRestFilter?.SelectedItem as RestaurantComboItem;
                dgFoodItems.ItemsSource = QueryFoodItems(filter?.RestaurantId > 0 ? filter.RestaurantId : null);
            }
            catch (Exception ex) { ShowError(ex); }
        }

        private void LoadFoodRestaurantFilter()
        {
            if (cbFoodRestFilter == null) return;
            var current = cbFoodRestFilter.SelectedItem;
            var items = new List<RestaurantComboItem> { new() { RestaurantId = 0, DisplayText = "All Restaurants" } };
            items.AddRange(GetAllRestaurants().Select(r => new RestaurantComboItem { RestaurantId = r.RestaurantId, DisplayText = r.Name }));
            cbFoodRestFilter.DisplayMemberPath = "DisplayText";
            cbFoodRestFilter.ItemsSource       = items;
            if (current == null) cbFoodRestFilter.SelectedIndex = 0;
        }

        // Raw SQL — avoids EF circular navigation via FoodItem.Restaurant
        private List<FoodItemDto> QueryFoodItems(int? restaurantId)
        {
            var results = new List<FoodItemDto>();
            using var conn = new SqlConnection(_db.Database.GetConnectionString());
            conn.Open();
            var sql = @"
                SELECT f.FoodItemId, f.RestaurantId, f.Name, f.Description, f.Price,
                       f.Category, f.IsAvailable, f.DailyQuantityLimit, f.DailyQuantityUsed,
                       ISNULL(r.Name, '') AS RestaurantName
                FROM FoodItems f
                LEFT JOIN Restaurants r ON f.RestaurantId = r.RestaurantId"
                + (restaurantId.HasValue ? " WHERE f.RestaurantId = @rid" : "")
                + " ORDER BY f.RestaurantId, f.Name";
            using var cmd = new SqlCommand(sql, conn);
            if (restaurantId.HasValue) cmd.Parameters.AddWithValue("@rid", restaurantId.Value);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                results.Add(new FoodItemDto
                {
                    FoodItemId         = reader.GetInt32(0),
                    RestaurantId       = reader.GetInt32(1),
                    Name               = reader.GetString(2),
                    Description        = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Price              = reader.GetDecimal(4),
                    Category           = reader.IsDBNull(5) ? null : reader.GetString(5),
                    IsAvailable        = reader.GetBoolean(6),
                    DailyQuantityLimit = reader.IsDBNull(7) ? null : reader.GetInt32(7),
                    DailyQuantityUsed  = reader.GetInt32(8),
                    RestaurantName     = reader.GetString(9)
                });
            return results;
        }

        private void cbFoodRestFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFoodRestFilter?.SelectedItem is RestaurantComboItem r)
                try { dgFoodItems.ItemsSource = QueryFoodItems(r.RestaurantId > 0 ? r.RestaurantId : null); } catch { }
        }

        private void btnToggleAvailability_Click(object sender, RoutedEventArgs e)
        {
            if (dgFoodItems.SelectedItem is not FoodItemDto selected) { MessageBox.Show("Select a food item.", "Info", MessageBoxButton.OK, MessageBoxImage.Information); return; }
            try
            {
                var food = _db.FoodItems.Find(selected.FoodItemId) ?? throw new InvalidOperationException("Food item not found.");
                food.IsAvailable = !food.IsAvailable;
                _db.SaveChanges();
                LoadFoodItems();
            }
            catch (Exception ex) { ShowError(ex); }
        }

        // ═══════════════════════════════════════════
        //  LOGOUT
        // ═══════════════════════════════════════════

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Logout?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            { new LoginWindow().Show(); this.Close(); }
        }

        // ─── Shared helper ───────────────────────────
        private static void ShowError(Exception ex) =>
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    // ─── DTOs (flat, no navigation properties — safe for DataGrid binding) ───
    public class RestaurantDto
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string OwnerName { get; set; } = "";
        public bool IsOpen { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalOrders { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public int OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FoodItemDto
    {
        public int FoodItemId { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public bool IsAvailable { get; set; }
        public int? DailyQuantityLimit { get; set; }
        public int DailyQuantityUsed { get; set; }
        public string RestaurantName { get; set; } = "";
    }

    public class OwnerDto        { public int OwnerId { get; set; } public string FullName { get; set; } = ""; }
    public class DashboardRow    { public string Metric { get; set; } = ""; public string Value { get; set; } = ""; }
    public class OwnerComboItem  { public int OwnerId { get; set; } public string DisplayText { get; set; } = ""; }
    public class RestaurantComboItem { public int RestaurantId { get; set; } public string DisplayText { get; set; } = ""; }
}

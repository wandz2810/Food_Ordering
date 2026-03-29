using System;
using System.Collections.Generic;
using System.Linq;
using Food_Ordering.Entities;
using Microsoft.EntityFrameworkCore;

namespace Food_Ordering.Service
{
    public class AdminService
    {
        private readonly FoodOrderingDbContext _context;

        public AdminService()
        {
            _context = new FoodOrderingDbContext();
        }

        // ═══════════════════════════════════════════
        //  DASHBOARD
        // ═══════════════════════════════════════════

        public int GetUserCount() => _context.Users.Count();
        public int GetCustomerCount() => _context.Customers.Count();
        public int GetShipperCount() => _context.Shippers.Count();
        public int GetRestaurantOwnerCount() => _context.RestaurantOwners.Count();
        public int GetRestaurantCount() => _context.Restaurants.Count();
        public int GetOrderCount() => _context.Orders.Count();
        public int GetFoodItemCount() => _context.FoodItems.Count();

        public decimal GetTotalRevenue()
        {
            return _context.Orders
                .Where(o => o.Status == "Delivered")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;
        }

        public int GetPendingOrderCount()
        {
            return _context.Orders.Count(o => o.Status == "PendingPayment" || o.Status == "WaitingShipper" || o.Status == "WaitingFood");
        }

        public int GetPendingApplicationCount()
        {
            var shipperApps = _context.ShipperApplications.Count(a => a.Status == "Pending");
            var restApps = _context.RestaurantApplications.Count(a => a.Status == "Pending");
            return shipperApps + restApps;
        }

        // ═══════════════════════════════════════════
        //  USERS
        // ═══════════════════════════════════════════

        public List<User> GetAllUsers()
        {
            return _context.Users.OrderByDescending(u => u.CreatedAt).ToList();
        }

        public List<User> SearchUsers(string keyword, string? roleFilter)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(u =>
                    u.Email.ToLower().Contains(keyword) ||
                    u.FullName.ToLower().Contains(keyword) ||
                    u.PhoneNumber.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "All")
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            return query.OrderByDescending(u => u.CreatedAt).ToList();
        }

        public void CreateUser(User user, string role, string? address = null,
            string? vehicleType = null, string? licensePlate = null)
        {
            if (_context.Users.Any(u => u.Email.ToLower() == user.Email.ToLower()))
                throw new InvalidOperationException("Email already exists.");

            user.Role = role;
            user.Status = "Active";
            user.CreatedAt = DateTime.Now;
            _context.Users.Add(user);
            _context.SaveChanges();

            switch (role)
            {
                case "Customer":
                    _context.Customers.Add(new Customer
                    {
                        CustomerId = user.UserId,
                        Address = address
                    });
                    break;
                case "Shipper":
                    _context.Shippers.Add(new Entities.Shipper
                    {
                        ShipperId = user.UserId,
                        VehicleType = vehicleType ?? "motorbike",
                        LicensePlate = licensePlate ?? "",
                        IsAvailable = true
                    });
                    break;
                case "RestaurantOwner":
                    _context.RestaurantOwners.Add(new RestaurantOwner
                    {
                        OwnerId = user.UserId
                    });
                    break;
            }
            _context.SaveChanges();
        }

        public void UpdateUser(int userId, string fullName, string phoneNumber, string status)
        {
            var user = _context.Users.Find(userId);
            if (user == null) throw new InvalidOperationException("User not found.");

            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            user.Status = status;
            _context.SaveChanges();
        }

        public void DeleteUser(int userId)
        {
            var user = _context.Users
                .Include(u => u.Customer)
                .Include(u => u.Shipper)
                .Include(u => u.RestaurantOwner)
                .FirstOrDefault(u => u.UserId == userId);

            if (user == null) throw new InvalidOperationException("User not found.");

            // Remove role-specific entities
            if (user.Customer != null)
            {
                var cart = _context.Carts.FirstOrDefault(c => c.CustomerId == userId);
                if (cart != null)
                {
                    var cartItems = _context.CartItems.Where(ci => ci.CartId == cart.CartId);
                    _context.CartItems.RemoveRange(cartItems);
                    _context.Carts.Remove(cart);
                }
                _context.Customers.Remove(user.Customer);
            }
            if (user.Shipper != null)
                _context.Shippers.Remove(user.Shipper);
            if (user.RestaurantOwner != null)
            {
                var restaurant = _context.Restaurants.FirstOrDefault(r => r.OwnerId == userId);
                if (restaurant != null)
                {
                    var foodItems = _context.FoodItems.Where(f => f.RestaurantId == restaurant.RestaurantId);
                    _context.FoodItems.RemoveRange(foodItems);
                    _context.Restaurants.Remove(restaurant);
                }
                _context.RestaurantOwners.Remove(user.RestaurantOwner);
            }

            // Remove applications
            var shipperApps = _context.ShipperApplications.Where(a => a.UserId == userId);
            _context.ShipperApplications.RemoveRange(shipperApps);
            var restApps = _context.RestaurantApplications.Where(a => a.UserId == userId);
            _context.RestaurantApplications.RemoveRange(restApps);

            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        // ═══════════════════════════════════════════
        //  RESTAURANTS
        // ═══════════════════════════════════════════

        public List<Restaurant> GetAllRestaurants()
        {
            return _context.Restaurants
                .Include(r => r.Owner)
                    .ThenInclude(o => o.Owner)
                .OrderByDescending(r => r.CreatedAt)
                .ToList();
        }

        /// <summary>
        /// Get owners who do not yet have a restaurant
        /// </summary>
        public List<RestaurantOwner> GetAvailableOwners()
        {
            return _context.RestaurantOwners
                .Include(o => o.Owner)
                .Where(o => o.Restaurant == null)
                .ToList();
        }

        public void CreateRestaurant(Restaurant restaurant)
        {
            if (_context.Restaurants.Any(r => r.OwnerId == restaurant.OwnerId))
                throw new InvalidOperationException("This owner already has a restaurant.");

            restaurant.CreatedAt = DateTime.Now;
            restaurant.IsOpen = true;
            restaurant.AverageRating = 0;
            restaurant.TotalOrders = 0;
            _context.Restaurants.Add(restaurant);
            _context.SaveChanges();
        }

        public void UpdateRestaurant(Restaurant restaurant)
        {
            var existing = _context.Restaurants.Find(restaurant.RestaurantId);
            if (existing == null) throw new InvalidOperationException("Restaurant not found.");

            existing.Name = restaurant.Name;
            existing.Address = restaurant.Address;
            existing.Description = restaurant.Description;
            existing.LogoUrl = restaurant.LogoUrl;
            existing.IsOpen = restaurant.IsOpen;
            existing.Latitude = restaurant.Latitude;
            existing.Longitude = restaurant.Longitude;
            _context.SaveChanges();
        }

        public void DeleteRestaurant(int restaurantId)
        {
            var restaurant = _context.Restaurants.Find(restaurantId);
            if (restaurant == null) throw new InvalidOperationException("Restaurant not found.");

            // Remove related food items
            var foodItems = _context.FoodItems.Where(f => f.RestaurantId == restaurantId);
            _context.FoodItems.RemoveRange(foodItems);

            _context.Restaurants.Remove(restaurant);
            _context.SaveChanges();
        }

        // ═══════════════════════════════════════════
        //  ORDERS
        // ═══════════════════════════════════════════

        public List<Order> GetAllOrders()
        {
            return _context.Orders
                .Include(o => o.Customer).ThenInclude(c => c.CustomerNavigation)
                .Include(o => o.Restaurant)
                .Include(o => o.Shipper).ThenInclude(s => s != null ? s.ShipperNavigation : null)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
        }

        public List<Order> SearchOrders(string? statusFilter)
        {
            var query = _context.Orders
                .Include(o => o.Customer).ThenInclude(c => c.CustomerNavigation)
                .Include(o => o.Restaurant)
                .Include(o => o.Shipper).ThenInclude(s => s != null ? s.ShipperNavigation : null)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            return query.OrderByDescending(o => o.CreatedAt).ToList();
        }

        public void UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = _context.Orders.Find(orderId);
            if (order == null) throw new InvalidOperationException("Order not found.");

            order.Status = newStatus;
            order.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }

        // ═══════════════════════════════════════════
        //  FOOD ITEMS
        // ═══════════════════════════════════════════

        public List<FoodItem> GetAllFoodItems()
        {
            return _context.FoodItems
                .Include(f => f.Restaurant)
                .OrderBy(f => f.RestaurantId)
                .ThenBy(f => f.Name)
                .ToList();
        }

        public List<FoodItem> GetFoodItemsByRestaurant(int restaurantId)
        {
            return _context.FoodItems
                .Include(f => f.Restaurant)
                .Where(f => f.RestaurantId == restaurantId)
                .OrderBy(f => f.Name)
                .ToList();
        }

        public void ToggleFoodAvailability(int foodItemId)
        {
            var food = _context.FoodItems.Find(foodItemId);
            if (food == null) throw new InvalidOperationException("Food item not found.");

            food.IsAvailable = !food.IsAvailable;
            _context.SaveChanges();
        }

        // ═══════════════════════════════════════════
        //  APPLICATIONS
        // ═══════════════════════════════════════════

        public List<ShipperApplication> GetShipperApplications(string? statusFilter = null)
        {
            var query = _context.ShipperApplications
                .Include(a => a.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                query = query.Where(a => a.Status == statusFilter);

            return query.OrderByDescending(a => a.SubmittedAt).ToList();
        }

        public List<RestaurantApplication> GetRestaurantApplications(string? statusFilter = null)
        {
            var query = _context.RestaurantApplications
                .Include(a => a.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All")
                query = query.Where(a => a.Status == statusFilter);

            return query.OrderByDescending(a => a.SubmittedAt).ToList();
        }

        public void ApproveShipperApplication(int applicationId, string? adminNote = null)
        {
            var app = _context.ShipperApplications.Include(a => a.User).FirstOrDefault(a => a.ApplicationId == applicationId);
            if (app == null) throw new InvalidOperationException("Application not found.");

            app.Status = "Approved";
            app.AdminNote = adminNote;
            app.ReviewedAt = DateTime.Now;

            // Update user role if needed
            var user = app.User;
            if (user.Role != "Shipper")
            {
                user.Role = "Shipper";
            }

            // Create Shipper record if not exists
            if (!_context.Shippers.Any(s => s.ShipperId == user.UserId))
            {
                _context.Shippers.Add(new Entities.Shipper
                {
                    ShipperId = user.UserId,
                    VehicleType = app.VehicleType,
                    LicensePlate = app.LicensePlate,
                    IsAvailable = true
                });
            }

            _context.SaveChanges();
        }

        public void RejectShipperApplication(int applicationId, string? adminNote = null)
        {
            var app = _context.ShipperApplications.Find(applicationId);
            if (app == null) throw new InvalidOperationException("Application not found.");

            app.Status = "Rejected";
            app.AdminNote = adminNote;
            app.ReviewedAt = DateTime.Now;
            _context.SaveChanges();
        }

        public void ApproveRestaurantApplication(int applicationId, string? adminNote = null)
        {
            var app = _context.RestaurantApplications.Include(a => a.User).FirstOrDefault(a => a.ApplicationId == applicationId);
            if (app == null) throw new InvalidOperationException("Application not found.");

            app.Status = "Approved";
            app.AdminNote = adminNote;
            app.ReviewedAt = DateTime.Now;

            var user = app.User;

            // Create RestaurantOwner if not exists
            if (!_context.RestaurantOwners.Any(o => o.OwnerId == user.UserId))
            {
                user.Role = "RestaurantOwner";
                _context.RestaurantOwners.Add(new RestaurantOwner
                {
                    OwnerId = user.UserId
                });
            }

            // Create Restaurant from application data
            if (!_context.Restaurants.Any(r => r.OwnerId == user.UserId))
            {
                _context.Restaurants.Add(new Restaurant
                {
                    OwnerId = user.UserId,
                    Name = app.RestaurantName,
                    Address = app.Address,
                    Latitude = app.Latitude,
                    Longitude = app.Longitude,
                    Description = app.Description,
                    LogoUrl = app.LogoUrl,
                    OpenTime = app.OpenTime,
                    CloseTime = app.CloseTime,
                    IsOpen = true,
                    AverageRating = 0,
                    TotalOrders = 0,
                    CreatedAt = DateTime.Now
                });
            }

            _context.SaveChanges();
        }

        public void RejectRestaurantApplication(int applicationId, string? adminNote = null)
        {
            var app = _context.RestaurantApplications.Find(applicationId);
            if (app == null) throw new InvalidOperationException("Application not found.");

            app.Status = "Rejected";
            app.AdminNote = adminNote;
            app.ReviewedAt = DateTime.Now;
            _context.SaveChanges();
        }
    }
}

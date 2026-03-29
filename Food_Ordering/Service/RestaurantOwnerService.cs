using System;
using System.Collections.Generic;
using System.Linq;
using Food_Ordering.Entities;

namespace Food_Ordering
{
    public class RestaurantOwnerService
    {
        private readonly FoodOrderingDbContext _context;
        private User? _currentAccount;

        // Constructor không tham số — an toàn khi gọi từ UI, sẽ cố lấy user từ App.CurrentUserId nếu đã set
        public RestaurantOwnerService()
        {
            _context = new FoodOrderingDbContext();

            if (_currentAccount.UserId > 0)
            {
                // tìm user hiện tại trong DB (có thể null nếu App.CurrentUserId chưa hợp lệ)
                _currentAccount = _context.Users.Find(_currentAccount.UserId);
            }
        }

        // Constructor có tham số khi bạn đã có object User và muốn inject vào service
        public RestaurantOwnerService(User user)
        {
            _context = new FoodOrderingDbContext();
            _currentAccount = user;
        }

        // 1. Lấy nhà hàng của owner
        public Restaurant? GetRestaurant(int ownerId)
        {
            return _context.Restaurants
                .FirstOrDefault(r => r.OwnerId == ownerId);
        }

        // 2. Thêm món ăn
        public void AddFood(FoodItem food)
        {
            if (food == null) throw new ArgumentNullException(nameof(food));
            _context.FoodItems.Add(food);
            _context.SaveChanges();
        }

        // 3. Lấy menu theo RestaurantId
        public List<FoodItem> GetMenu(int restaurantId)
        {
            return _context.FoodItems
                .Where(f => f.RestaurantId == restaurantId)
                .ToList();
        }

        // 4. Cập nhật món ăn
        public void UpdateFood(FoodItem food)
        {
            if (food == null) throw new ArgumentNullException(nameof(food));
            var existing = _context.FoodItems.Find(food.FoodItemId);
            if (existing == null) throw new InvalidOperationException("Food item not found.");

            existing.Name = food.Name;
            existing.Price = food.Price;
            existing.Description = food.Description;
            existing.Category = food.Category;
            existing.ImageUrl = food.ImageUrl;
            existing.IsAvailable = food.IsAvailable;
            existing.DailyQuantityLimit = food.DailyQuantityLimit;
            existing.DailyQuantityUsed = food.DailyQuantityUsed;
            existing.QuantityResetDate = food.QuantityResetDate;
            existing.RestaurantId = food.RestaurantId;

            _context.SaveChanges();
        }

        // 5. Xóa món ăn
        public void DeleteFood(int id)
        {
            var food = _context.FoodItems.Find(id);
            if (food == null) throw new InvalidOperationException("Food item not found.");
            _context.FoodItems.Remove(food);
            _context.SaveChanges();
        }

        // MỚI: Đánh dấu "hết trong ngày" thủ công
        public void MarkSoldOutForToday(int foodItemId)
        {
            var food = _context.FoodItems.Find(foodItemId);
            if (food == null) throw new InvalidOperationException("Food item not found.");

            food.IsAvailable = false;
            food.QuantityResetDate = DateOnly.FromDateTime(DateTime.Today);

            if (food.DailyQuantityLimit.HasValue)
            {
                food.DailyQuantityUsed = food.DailyQuantityLimit.Value;
            }

            _context.SaveChanges();
        }

        // MỚI: Mở lại món (bỏ trạng thái hết trong ngày)
        public void RestoreAvailability(int foodItemId)
        {
            var food = _context.FoodItems.Find(foodItemId);
            if (food == null) throw new InvalidOperationException("Food item not found.");

            food.IsAvailable = true;
            food.QuantityResetDate = null;
            food.DailyQuantityUsed = 0;

            _context.SaveChanges();
        }
    }
}
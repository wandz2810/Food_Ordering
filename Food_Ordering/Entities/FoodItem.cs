using System;
using System.Collections.Generic;

namespace Food_Ordering.Entities;

public partial class FoodItem
{
    public int FoodItemId { get; set; }

    public int RestaurantId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public string? Category { get; set; }

    public bool IsAvailable { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public int Quantity { get; set; } = 1;

    public int? DailyQuantityLimit { get; set; }

    public int DailyQuantityUsed { get; set; }

    public DateOnly? QuantityResetDate { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Restaurant Restaurant { get; set; } = null!;
}

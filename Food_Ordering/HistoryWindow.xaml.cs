using Food_Ordering.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;

namespace Food_Ordering
{
    public partial class HistoryWindow : Window
    {
        private readonly FoodOrderingDbContext _context = new FoodOrderingDbContext();

        public HistoryWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            // Liem lấy đúng cái ID mà bạn của Liem đã lưu lúc nãy
            int currentId = App.CurrentUserId;

            var myOrders = _context.Orders
                .Where(o => o.CustomerId == currentId) 
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            dgOrders.ItemsSource = myOrders;
        }

        // Khi nhấn vào một dòng trong DataGrid, hiện chi tiết các món của đơn đó
        private void DgOrders_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (dgOrders.SelectedItem is Order selectedOrder)
            {
                var details = _context.OrderItems
                    .Where(oi => oi.OrderId == selectedOrder.OrderId)
                    .ToList();

                lbOrderDetails.ItemsSource = details;
            }
        }
    }
}
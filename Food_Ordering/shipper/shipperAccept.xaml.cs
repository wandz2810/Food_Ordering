using System.Linq;
using System.Windows;
using Food_Ordering.Entities;

namespace Food_Ordering.shipper
{
    public partial class shipperAccept : Window
    {
        private readonly int _orderId;

        public shipperAccept(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            LoadOrder();
        }

        private void LoadOrder()
        {
            try
            {
                using FoodOrderingDbContext context = new FoodOrderingDbContext();

                var order = context.Orders.FirstOrDefault(o => o.OrderId == _orderId);

                if (order == null)
                {
                    MessageBox.Show("Order not found.");
                    this.Close();
                    return;
                }

                txtOrderId.Text = order.OrderId.ToString();
                txtRestaurantId.Text = order.RestaurantId.ToString();
                txtTotalAmount.Text = order.TotalAmount.ToString("N0") + " VND";
                txtDeliveryAddress.Text = order.DeliveryAddress;
                txtNote.Text = string.IsNullOrWhiteSpace(order.Note) ? "No note" : order.Note;
                txtStatus.Text = order.Status;

                if (order.Status == "PickedUp")
                {
                    btnPickedUp.Visibility = Visibility.Collapsed;
                    btnDelivered.Visibility = Visibility.Visible;
                }
                else if (order.Status == "Delivered")
                {
                    btnPickedUp.Visibility = Visibility.Collapsed;
                    btnDelivered.Visibility = Visibility.Collapsed;
                }
                else
                {
                    btnPickedUp.Visibility = Visibility.Visible;
                    btnDelivered.Visibility = Visibility.Collapsed;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error loading order: " + ex.Message);
            }
        }

        private void btnPickedUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using FoodOrderingDbContext context = new FoodOrderingDbContext();

                var order = context.Orders.FirstOrDefault(o => o.OrderId == _orderId);

                if (order == null)
                {
                    MessageBox.Show("Order not found.");
                    return;
                }

                order.Status = "PickedUp";
                context.SaveChanges();

                MessageBox.Show("Lấy hàng thành công.");

                txtStatus.Text = "PickedUp";
                btnPickedUp.Visibility = Visibility.Collapsed;
                btnDelivered.Visibility = Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnDelivered_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using FoodOrderingDbContext context = new FoodOrderingDbContext();

                var order = context.Orders.FirstOrDefault(o => o.OrderId == _orderId);

                if (order == null)
                {
                    MessageBox.Show("Order not found.");
                    return;
                }

                if (order.Status != "Shipping")
                {
                    MessageBox.Show("Đơn hàng chưa ở trạng thái đang giao.");
                    return;
                }

                order.Status = "Completed";
                context.SaveChanges();

                MessageBox.Show("Giao hàng thành công.");

                Shipper shipperWindow = new Shipper();
                shipperWindow.Show();
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
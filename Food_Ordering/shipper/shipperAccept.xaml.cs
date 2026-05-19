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
                    MessageBox.Show("Không tìm thấy đơn hàng.");
                    this.Close();
                    return;
                }

                txtOrderId.Text = order.OrderId.ToString();
                txtRestaurantId.Text = order.RestaurantId.ToString();
                txtTotalAmount.Text = order.TotalAmount.ToString("N0") + " VND";
                txtDeliveryAddress.Text = order.DeliveryAddress;
                txtNote.Text = string.IsNullOrWhiteSpace(order.Note) ? "Không có ghi chú" : order.Note;
                txtStatus.Text = order.Status;

                // LOGIC HIỂN THỊ NÚT THEO TRẠNG THÁI TIẾNG VIỆT
                if (order.Status == "Đang chờ tài xế")
                {
                    btnPickedUp.Visibility = Visibility.Visible;
                    btnDelivered.Visibility = Visibility.Collapsed;
                }
                else if (order.Status == "Đã lấy hàng")
                {
                    btnPickedUp.Visibility = Visibility.Collapsed;
                    btnDelivered.Visibility = Visibility.Visible;
                }
                else
                {
                    btnPickedUp.Visibility = Visibility.Collapsed;
                    btnDelivered.Visibility = Visibility.Collapsed;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi tải đơn hàng: " + ex.Message);
            }
        }

        private void btnPickedUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using FoodOrderingDbContext context = new FoodOrderingDbContext();
                var order = context.Orders.FirstOrDefault(o => o.OrderId == _orderId);

                if (order == null) return;

                if (order.Status != "Đang chờ tài xế")
                {
                    MessageBox.Show("Chỉ đơn hàng đang chờ mới được lấy hàng.");
                    return;
                }

                // Cập nhật trạng thái sang tiếng Việt
                order.Status = "Đã lấy hàng";
                context.SaveChanges();

                MessageBox.Show("Lấy hàng thành công.");

                txtStatus.Text = "Đã lấy hàng";
                btnPickedUp.Visibility = Visibility.Collapsed;
                btnDelivered.Visibility = Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnDelivered_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using FoodOrderingDbContext context = new FoodOrderingDbContext();
                var order = context.Orders.FirstOrDefault(o => o.OrderId == _orderId);

                if (order == null) return;

                if (order.Status != "Đã lấy hàng")
                {
                    MessageBox.Show("Phải lấy hàng trước khi xác nhận giao xong.");
                    return;
                }

                // Cập nhật trạng thái cuối cùng sang tiếng Việt
                order.Status = "Đã giao hàng";
                context.SaveChanges();

                MessageBox.Show("Đơn hàng đã hoàn thành!");

                // Quay lại màn hình danh sách của Shipper
                Shipper shipperWindow = new Shipper();
                shipperWindow.Show();
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}
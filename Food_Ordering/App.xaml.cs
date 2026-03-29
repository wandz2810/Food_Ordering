using System.Configuration;
using System.Data;
using System.Windows;

namespace Food_Ordering
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // ID user hiện tại (set khi login). Mặc định 0 = chưa login.
        public static int CurrentUserId { get; set; } = 0;

    }

}

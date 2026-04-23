namespace Booking_Homestay.Models.ViewModels
{
    public class QuanTriDashboardViewModel
    {
        public int TongDatPhong { get; set; }
        public int DatPhongHomNay { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal DoanhThuThang { get; set; }
        public int PhongDangSuDung { get; set; }
        public int TongPhong { get; set; }
        public int ChoXacNhan { get; set; }
        public List<DatPhong> DatPhongGanDay { get; set; } = new();
        public Dictionary<string, decimal> DoanhThuTheoNgay { get; set; } = new();
    }
}

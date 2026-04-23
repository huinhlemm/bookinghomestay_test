namespace Booking_Homestay.Models.ViewModels
{
    public class ChiTietPhongViewModel
    {
        public Phong Phong { get; set; } = null!;
        public List<GiaPhong> DanhSachGia { get; set; } = new();
        public List<KhungGio> DanhSachKhungGio { get; set; } = new();
        public List<Phong> PhongLienQuan { get; set; } = new();
        public Dictionary<string, string?> CaiDat { get; set; } = new();
    }
}

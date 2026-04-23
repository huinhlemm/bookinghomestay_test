namespace Booking_Homestay.Models.ViewModels
{
    public class TrangChuViewModel
    {
        public List<KhuVuc> DanhSachKhuVuc { get; set; } = new();
        public List<Phong> DanhSachPhong { get; set; } = new();
        public List<LoaiPhong> DanhSachLoaiPhong { get; set; } = new();
        public Dictionary<string, string?> CaiDat { get; set; } = new();
    }
}

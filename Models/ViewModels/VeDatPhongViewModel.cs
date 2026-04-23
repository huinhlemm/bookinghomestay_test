namespace Booking_Homestay.Models.ViewModels
{
    public class VeDatPhongViewModel
    {
        public DatPhong DatPhong { get; set; } = null!;
        public Phong Phong { get; set; } = null!;
        public List<ChiTietDatPhong> DanhSachChiTiet { get; set; } = new();
        public string? QrCodeBase64 { get; set; }
        public bool HopLe { get; set; }
        public string? LoiThongBao { get; set; }
        public Dictionary<string, string?> CaiDat { get; set; } = new();
    }
}

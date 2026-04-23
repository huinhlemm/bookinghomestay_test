using System.ComponentModel.DataAnnotations;

namespace Booking_Homestay.Models.ViewModels
{
    public class DatPhongViewModel
    {
        [Required]
        public int MaPhong { get; set; }

        public string? TenPhong { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khách")]
        [Display(Name = "Tên khách")]
        public string TenKhach { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày")]
        [Display(Name = "Ngày nhận phòng")]
        public DateTime NgayNhanPhong { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn ít nhất 1 khung giờ")]
        [Display(Name = "Khung giờ đã chọn")]
        public List<int> DanhSachKhungGioId { get; set; } = new();

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Display(Name = "CCCD mặt trước")]
        public string? CccdMatTruoc { get; set; }

        [Display(Name = "CCCD mặt sau")]
        public string? CccdMatSau { get; set; }

        [Display(Name = "Nguồn đặt")]
        public string NguonDat { get; set; } = "Web";

        // Display data
        public Phong? ThongTinPhong { get; set; }
        public List<KhungGioTrangThai> DanhSachKhungGio { get; set; } = new();
        public decimal TongTien { get; set; }
        public Dictionary<string, string?> CaiDat { get; set; } = new();
    }

    public class KhungGioTrangThai
    {
        public KhungGio KhungGio { get; set; } = null!;
        public decimal Gia { get; set; }
        public decimal? GiaGoc { get; set; }
        public bool DaDat { get; set; }
        public bool DangChon { get; set; }
    }
}

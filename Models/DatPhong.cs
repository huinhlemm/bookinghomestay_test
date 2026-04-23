using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking_Homestay.Models
{
    public class DatPhong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Mã đặt phòng")]
        public string MaDatPhong { get; set; } = string.Empty; // e.g. "DP-20260421-001"

        [Display(Name = "Phòng")]
        public int MaPhong { get; set; }

        [Required]
        [Display(Name = "Tên khách")]
        public string TenKhach { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Ngày nhận phòng")]
        public DateTime NgayNhanPhong { get; set; }

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "ChoXacNhan"; // ChoXacNhan, DaXacNhan, DaNhanPhong, HoanThanh, DaHuy

        [Display(Name = "Tổng tiền")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal TongTien { get; set; }

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Display(Name = "Token bảo mật")]
        public string? Token { get; set; }

        [Display(Name = "Mã mở cửa")]
        public string? MaMoCua { get; set; }

        [Display(Name = "Thời gian nhận")]
        public DateTime? ThoiGianNhan { get; set; }

        [Display(Name = "Thời gian trả")]
        public DateTime? ThoiGianTra { get; set; }

        [Display(Name = "Nguồn đặt")]
        public string NguonDat { get; set; } = "Web"; // Web, Messenger, Zalo

        [Display(Name = "Minh chứng thanh toán")]
        public string? MinhChungThanhToan { get; set; }

        [Display(Name = "CCCD mặt trước")]
        public string? CccdMatTruoc { get; set; }

        [Display(Name = "CCCD mặt sau")]
        public string? CccdMatSau { get; set; }

        // Navigation
        [ForeignKey("MaPhong")]
        public Phong? Phong { get; set; }

        public ICollection<ChiTietDatPhong>? DanhSachChiTiet { get; set; }
        public ICollection<ThanhToan>? DanhSachThanhToan { get; set; }
    }
}

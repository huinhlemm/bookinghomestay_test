using System.ComponentModel.DataAnnotations;

namespace Booking_Homestay.Models
{
    public class TaiKhoan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Mật khẩu (hash)")]
        public string MatKhauHash { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "NhanVien"; // Admin, NhanVien

        [Display(Name = "Đã duyệt")]
        public bool DaDuyet { get; set; } = false;

        [Display(Name = "Bị khóa")]
        public bool BiKhoa { get; set; } = false;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}

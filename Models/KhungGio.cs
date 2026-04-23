using System.ComponentModel.DataAnnotations;

namespace Booking_Homestay.Models
{
    public class KhungGio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên khung giờ")]
        public string TenKhungGio { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan GioBatDau { get; set; }

        [Required]
        [Display(Name = "Giờ kết thúc")]
        public TimeSpan GioKetThuc { get; set; }

        [Display(Name = "Biểu tượng")]
        public string? BieuTuong { get; set; }

        [Display(Name = "Thứ tự")]
        public int ThuTu { get; set; }

        // Navigation
        public ICollection<GiaPhong>? DanhSachGia { get; set; }
        public ICollection<ChiTietDatPhong>? DanhSachChiTiet { get; set; }
    }
}

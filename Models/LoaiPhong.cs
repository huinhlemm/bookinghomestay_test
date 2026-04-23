using System.ComponentModel.DataAnnotations;

namespace Booking_Homestay.Models
{
    public class LoaiPhong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên loại phòng")]
        public string TenLoaiPhong { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Biểu tượng")]
        public string? BieuTuong { get; set; }

        // Navigation
        public ICollection<Phong>? DanhSachPhong { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Booking_Homestay.Models
{
    public class KhuVuc
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên khu vực")]
        public string TenKhuVuc { get; set; } = string.Empty;

        [Display(Name = "Thành phố")]
        public string? ThanhPho { get; set; }

        [Display(Name = "Quận/Huyện")]
        public string? QuanHuyen { get; set; }

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? HinhAnh { get; set; }

        // Navigation
        public ICollection<Phong>? DanhSachPhong { get; set; }
    }
}

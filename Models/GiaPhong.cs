using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking_Homestay.Models
{
    public class GiaPhong
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Phòng")]
        public int MaPhong { get; set; }

        [Display(Name = "Khung giờ")]
        public int MaKhungGio { get; set; }

        [Required]
        [Display(Name = "Giá")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal Gia { get; set; }

        [Display(Name = "Giá gốc")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal? GiaGoc { get; set; }

        [Display(Name = "Phần trăm giảm")]
        public int? PhanTramGiam { get; set; }

        // Navigation
        [ForeignKey("MaPhong")]
        public Phong? Phong { get; set; }

        [ForeignKey("MaKhungGio")]
        public KhungGio? KhungGio { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking_Homestay.Models
{
    public class ChiTietDatPhong
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Đặt phòng")]
        public int MaDatPhongId { get; set; }

        [Display(Name = "Khung giờ")]
        public int MaKhungGio { get; set; }

        [Display(Name = "Ngày áp dụng")]
        public DateTime NgayApDung { get; set; }

        [Display(Name = "Giá áp dụng")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaApDung { get; set; }

        // Navigation
        [ForeignKey("MaDatPhongId")]
        public DatPhong? DatPhong { get; set; }

        [ForeignKey("MaKhungGio")]
        public KhungGio? KhungGio { get; set; }
    }
}

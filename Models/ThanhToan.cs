using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking_Homestay.Models
{
    public class ThanhToan
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Đặt phòng")]
        public int MaDatPhongId { get; set; }

        [Display(Name = "Số tiền")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal SoTien { get; set; }

        [Display(Name = "Phương thức")]
        public string PhuongThuc { get; set; } = "TienMat"; // TienMat, ChuyenKhoan, MoMo, VNPay

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "ChuaThanhToan"; // ChuaThanhToan, DaThanhToan

        [Display(Name = "Ngày thanh toán")]
        public DateTime? NgayThanhToan { get; set; }

        // Navigation
        [ForeignKey("MaDatPhongId")]
        public DatPhong? DatPhong { get; set; }
    }
}

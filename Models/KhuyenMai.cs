using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking_Homestay.Models
{
    public class KhuyenMai
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên khuyến mãi")]
        public string TenKhuyenMai { get; set; } = string.Empty;

        [Display(Name = "Loại giảm")]
        public string LoaiGiam { get; set; } = "TheoKhungGio"; // TheoKhungGio, TheoSoGio

        [Display(Name = "Phần trăm giảm")]
        public int PhanTramGiam { get; set; }

        [Display(Name = "Số giờ tối thiểu")]
        public int? SoGioToiThieu { get; set; }

        [Display(Name = "Khung giờ áp dụng")]
        public int? MaKhungGio { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        public DateTime NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        public DateTime NgayKetThuc { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation
        [ForeignKey("MaKhungGio")]
        public KhungGio? KhungGio { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Booking_Homestay.Models
{
    public class CaiDatHeThong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên cài đặt")]
        public string TenCaiDat { get; set; } = string.Empty;

        [Display(Name = "Giá trị")]
        public string? GiaTri { get; set; }

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Nhóm")]
        public string NhomCaiDat { get; set; } = "ChungChung"; // ChungChung, LienHe, GiaoDien, NoiDung
    }
}

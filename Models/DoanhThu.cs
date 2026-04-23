using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking_Homestay.Models
{
    public class DoanhThu
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Tháng")]
        public int Thang { get; set; }

        [Display(Name = "Năm")]
        public int Nam { get; set; }

        [Display(Name = "Tổng doanh thu")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal TongDoanhThu { get; set; }

        [Display(Name = "Số đơn")]
        public int SoDon { get; set; }

        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }
}

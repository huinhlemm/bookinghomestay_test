using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking_Homestay.Models
{
    public class Phong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tên phòng")]
        public string TenPhong { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? HinhAnh { get; set; }

        [Display(Name = "Diện tích (m²)")]
        public double DienTich { get; set; }

        [Display(Name = "Số người tối đa")]
        public int SoNguoiToiDa { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [Display(Name = "Tiện nghi")]
        public string? TienNghi { get; set; } // JSON string: ["WiFi","Điều hòa",...]

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true; // true = hoạt động

        // Foreign Keys
        [Display(Name = "Loại phòng")]
        public int MaLoaiPhong { get; set; }

        [Display(Name = "Khu vực")]
        public int MaKhuVuc { get; set; }

        // Navigation
        [ForeignKey("MaLoaiPhong")]
        public LoaiPhong? LoaiPhong { get; set; }

        [ForeignKey("MaKhuVuc")]
        public KhuVuc? KhuVuc { get; set; }

        public ICollection<GiaPhong>? DanhSachGia { get; set; }
        public ICollection<DatPhong>? DanhSachDatPhong { get; set; }
    }
}

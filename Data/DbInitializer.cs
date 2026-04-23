using Booking_Homestay.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking_Homestay.Data
{
    public static class DbInitializer
    {
        public static void Initialize(UngDungDbContext context)
        {
            // Đảm bảo DB đã được tạo
            context.Database.EnsureCreated();

            // --- TaiKhoan admin mặc định ---
            if (!context.TaiKhoan.Any())
            {
                context.TaiKhoan.Add(new TaiKhoan
                {
                    TenDangNhap = "admin",
                    MatKhauHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    HoTen = "Administrator",
                    Email = "admin@homestay.vn",
                    VaiTro = "Admin",
                    DaDuyet = true,
                    BiKhoa = false,
                    NgayTao = DateTime.Now
                });
                context.SaveChanges();
            }

            if (!context.CaiDatHeThong.Any(c => c.TenCaiDat == "ThoiGianGiuPhong"))
            {
                context.CaiDatHeThong.Add(new CaiDatHeThong 
                { 
                    TenCaiDat = "ThoiGianGiuPhong", 
                    GiaTri = "30", 
                    MoTa = "Thời gian giữ phòng chờ duyệt (phút)", 
                    NhomCaiDat = "ChungChung" 
                });
                context.SaveChanges();
            }

            if (!context.CaiDatHeThong.Any(c => c.TenCaiDat == "QrThanhToan"))
            {
                context.CaiDatHeThong.AddRange(new[] {
                    new CaiDatHeThong { TenCaiDat = "QrThanhToan", GiaTri = "", MoTa = "Link ảnh QR Thanh toán", NhomCaiDat = "ChungChung" },
                    new CaiDatHeThong { TenCaiDat = "ThongTinChuyenKhoan", GiaTri = "Ngân hàng: ACB\nSTK: 123456789\nTên: NGUYEN VAN A", MoTa = "Thông tin chuyển khoản", NhomCaiDat = "ChungChung" }
                });
                context.SaveChanges();
            }

            // Nếu đã có dữ liệu thì không seed nữa
            if (context.LoaiPhong.Any()) return;

            // --- LoaiPhong ---
            var loaiPhongs = new LoaiPhong[]
            {
                new LoaiPhong { TenLoaiPhong = "Phòng đơn", MoTa = "Phòng cho 1-2 người", BieuTuong = "bi-house" },
                new LoaiPhong { TenLoaiPhong = "Phòng đôi", MoTa = "Phòng cho 2-3 người", BieuTuong = "bi-house-heart" },
                new LoaiPhong { TenLoaiPhong = "Phòng VIP", MoTa = "Phòng cao cấp đầy đủ tiện nghi", BieuTuong = "bi-star" },
                new LoaiPhong { TenLoaiPhong = "Studio", MoTa = "Phòng studio hiện đại", BieuTuong = "bi-building" }
            };
            context.LoaiPhong.AddRange(loaiPhongs);
            context.SaveChanges();

            // --- KhuVuc ---
            var khuVucs = new KhuVuc[]
            {
                new KhuVuc { TenKhuVuc = "Quận 1", ThanhPho = "TP. Hồ Chí Minh", QuanHuyen = "Quận 1", MoTa = "Trung tâm thành phố", HinhAnh = "/images/khuvuc-q1.jpg" },
                new KhuVuc { TenKhuVuc = "Quận 3", ThanhPho = "TP. Hồ Chí Minh", QuanHuyen = "Quận 3", MoTa = "Khu vực sầm uất", HinhAnh = "/images/khuvuc-q3.jpg" },
                new KhuVuc { TenKhuVuc = "Thủ Đức", ThanhPho = "TP. Hồ Chí Minh", QuanHuyen = "TP. Thủ Đức", MoTa = "Khu đô thị mới", HinhAnh = "/images/khuvuc-td.jpg" },
                new KhuVuc { TenKhuVuc = "Quận 7", ThanhPho = "TP. Hồ Chí Minh", QuanHuyen = "Quận 7", MoTa = "Khu Phú Mỹ Hưng", HinhAnh = "/images/khuvuc-q7.jpg" }
            };
            context.KhuVuc.AddRange(khuVucs);
            context.SaveChanges();

            // --- KhungGio ---
            var khungGios = new KhungGio[]
            {
                new KhungGio { TenKhungGio = "Sáng", GioBatDau = new TimeSpan(9, 0, 0), GioKetThuc = new TimeSpan(12, 0, 0), BieuTuong = "☀️", ThuTu = 1 },
                new KhungGio { TenKhungGio = "Chiều", GioBatDau = new TimeSpan(12, 30, 0), GioKetThuc = new TimeSpan(15, 30, 0), BieuTuong = "🌤️", ThuTu = 2 },
                new KhungGio { TenKhungGio = "Tối", GioBatDau = new TimeSpan(16, 0, 0), GioKetThuc = new TimeSpan(19, 0, 0), BieuTuong = "🌙", ThuTu = 3 },
                new KhungGio { TenKhungGio = "Qua đêm", GioBatDau = new TimeSpan(21, 0, 0), GioKetThuc = new TimeSpan(8, 0, 0), BieuTuong = "🌜", ThuTu = 4 }
            };
            context.KhungGio.AddRange(khungGios);
            context.SaveChanges();

            // --- Phong ---
            var phongs = new Phong[]
            {
                new Phong { TenPhong = "Cozy Studio Q1", MoTa = "Phòng studio ấm cúng tại trung tâm Quận 1, view đẹp, đầy đủ nội thất hiện đại.", HinhAnh = "/images/phong-1.jpg", DienTich = 25, SoNguoiToiDa = 2, DiaChi = "123 Nguyễn Huệ, Q.1, TP.HCM", TienNghi = "[\"WiFi\",\"Điều hòa\",\"Tủ lạnh\",\"TV\",\"Máy giặt\"]", TrangThai = true, MaLoaiPhong = loaiPhongs[3].Id, MaKhuVuc = khuVucs[0].Id },
                new Phong { TenPhong = "Sunny Room Q3", MoTa = "Phòng đôi tràn ngập ánh sáng, gần chợ Bàn Cờ, thuận tiện di chuyển.", HinhAnh = "/images/phong-2.jpg", DienTich = 30, SoNguoiToiDa = 3, DiaChi = "45 Võ Văn Tần, Q.3, TP.HCM", TienNghi = "[\"WiFi\",\"Điều hòa\",\"Tủ lạnh\",\"TV\",\"Bếp\",\"Ban công\"]", TrangThai = true, MaLoaiPhong = loaiPhongs[1].Id, MaKhuVuc = khuVucs[1].Id },
                new Phong { TenPhong = "VIP Suite Thủ Đức", MoTa = "Phòng VIP rộng rãi tại Thủ Đức, sân vườn riêng, yên tĩnh.", HinhAnh = "/images/phong-3.jpg", DienTich = 45, SoNguoiToiDa = 4, DiaChi = "78 Võ Văn Ngân, TP. Thủ Đức", TienNghi = "[\"WiFi\",\"Điều hòa\",\"Tủ lạnh\",\"TV\",\"Bếp\",\"Ban công\",\"Sân vườn\",\"Hồ bơi\"]", TrangThai = true, MaLoaiPhong = loaiPhongs[2].Id, MaKhuVuc = khuVucs[2].Id },
                new Phong { TenPhong = "Modern Flat Q7", MoTa = "Căn hộ hiện đại tại Phú Mỹ Hưng, tiện nghi cao cấp, gần công viên.", HinhAnh = "/images/phong-4.jpg", DienTich = 35, SoNguoiToiDa = 3, DiaChi = "12 Nguyễn Lương Bằng, Q.7, TP.HCM", TienNghi = "[\"WiFi\",\"Điều hòa\",\"Tủ lạnh\",\"TV\",\"Bếp\",\"Máy giặt\",\"Ban công\"]", TrangThai = true, MaLoaiPhong = loaiPhongs[1].Id, MaKhuVuc = khuVucs[3].Id },
                new Phong { TenPhong = "Rooftop Single Q1", MoTa = "Phòng đơn trên sân thượng, view toàn cảnh thành phố, lãng mạn.", HinhAnh = "/images/phong-5.jpg", DienTich = 20, SoNguoiToiDa = 2, DiaChi = "89 Lê Lợi, Q.1, TP.HCM", TienNghi = "[\"WiFi\",\"Điều hòa\",\"Tủ lạnh\",\"TV\"]", TrangThai = true, MaLoaiPhong = loaiPhongs[0].Id, MaKhuVuc = khuVucs[0].Id },
                new Phong { TenPhong = "Garden Villa Thủ Đức", MoTa = "Biệt thự mini có sân vườn xanh mát, phù hợp nghỉ dưỡng cuối tuần.", HinhAnh = "/images/phong-6.jpg", DienTich = 60, SoNguoiToiDa = 6, DiaChi = "156 Phạm Văn Đồng, TP. Thủ Đức", TienNghi = "[\"WiFi\",\"Điều hòa\",\"Tủ lạnh\",\"TV\",\"Bếp\",\"Sân vườn\",\"BBQ\",\"Hồ bơi\",\"Phòng khách\"]", TrangThai = true, MaLoaiPhong = loaiPhongs[2].Id, MaKhuVuc = khuVucs[2].Id }
            };
            context.Phong.AddRange(phongs);
            context.SaveChanges();

            // --- GiaPhong ---
            var giaPhongs = new GiaPhong[]
            {
                // Phong 1
                new GiaPhong { MaPhong = phongs[0].Id, MaKhungGio = khungGios[0].Id, Gia = 200000 },
                new GiaPhong { MaPhong = phongs[0].Id, MaKhungGio = khungGios[1].Id, Gia = 250000 },
                new GiaPhong { MaPhong = phongs[0].Id, MaKhungGio = khungGios[2].Id, Gia = 200000 },
                new GiaPhong { MaPhong = phongs[0].Id, MaKhungGio = khungGios[3].Id, Gia = 500000 },
                // Phong 2
                new GiaPhong { MaPhong = phongs[1].Id, MaKhungGio = khungGios[0].Id, Gia = 250000 },
                new GiaPhong { MaPhong = phongs[1].Id, MaKhungGio = khungGios[1].Id, Gia = 300000 },
                new GiaPhong { MaPhong = phongs[1].Id, MaKhungGio = khungGios[2].Id, Gia = 250000 },
                new GiaPhong { MaPhong = phongs[1].Id, MaKhungGio = khungGios[3].Id, Gia = 600000 },
                // Phong 3
                new GiaPhong { MaPhong = phongs[2].Id, MaKhungGio = khungGios[0].Id, Gia = 400000 },
                new GiaPhong { MaPhong = phongs[2].Id, MaKhungGio = khungGios[1].Id, Gia = 450000 },
                new GiaPhong { MaPhong = phongs[2].Id, MaKhungGio = khungGios[2].Id, Gia = 400000 },
                new GiaPhong { MaPhong = phongs[2].Id, MaKhungGio = khungGios[3].Id, Gia = 900000 },
                // Phong 4
                new GiaPhong { MaPhong = phongs[3].Id, MaKhungGio = khungGios[0].Id, Gia = 300000 },
                new GiaPhong { MaPhong = phongs[3].Id, MaKhungGio = khungGios[1].Id, Gia = 350000 },
                new GiaPhong { MaPhong = phongs[3].Id, MaKhungGio = khungGios[2].Id, Gia = 300000 },
                new GiaPhong { MaPhong = phongs[3].Id, MaKhungGio = khungGios[3].Id, Gia = 700000 },
                // Phong 5
                new GiaPhong { MaPhong = phongs[4].Id, MaKhungGio = khungGios[0].Id, Gia = 180000 },
                new GiaPhong { MaPhong = phongs[4].Id, MaKhungGio = khungGios[1].Id, Gia = 220000 },
                new GiaPhong { MaPhong = phongs[4].Id, MaKhungGio = khungGios[2].Id, Gia = 180000 },
                new GiaPhong { MaPhong = phongs[4].Id, MaKhungGio = khungGios[3].Id, Gia = 450000 },
                // Phong 6
                new GiaPhong { MaPhong = phongs[5].Id, MaKhungGio = khungGios[0].Id, Gia = 500000 },
                new GiaPhong { MaPhong = phongs[5].Id, MaKhungGio = khungGios[1].Id, Gia = 550000 },
                new GiaPhong { MaPhong = phongs[5].Id, MaKhungGio = khungGios[2].Id, Gia = 500000 },
                new GiaPhong { MaPhong = phongs[5].Id, MaKhungGio = khungGios[3].Id, Gia = 1200000 }
            };
            context.GiaPhong.AddRange(giaPhongs);
            context.SaveChanges();

            // --- CaiDatHeThong ---
            var caiDats = new CaiDatHeThong[]
            {
                new CaiDatHeThong { TenCaiDat = "TenWebsite", GiaTri = "HomeStay Booking", MoTa = "Tên Website", NhomCaiDat = "ChungChung" },
                new CaiDatHeThong { TenCaiDat = "Logo", GiaTri = "/images/logo.png", MoTa = "Logo Website", NhomCaiDat = "GiaoDien" },
                new CaiDatHeThong { TenCaiDat = "Hotline", GiaTri = "0909 123 456", MoTa = "Số Điện Thoại", NhomCaiDat = "LienHe" },
                new CaiDatHeThong { TenCaiDat = "Email", GiaTri = "contact@homestay.vn", MoTa = "Email Liên Hệ", NhomCaiDat = "LienHe" },
                new CaiDatHeThong { TenCaiDat = "DiaChi", GiaTri = "123 Nguyễn Huệ, Q.1, TP.HCM", MoTa = "Địa Chỉ", NhomCaiDat = "LienHe" },
                new CaiDatHeThong { TenCaiDat = "TenWifi", GiaTri = "HomeStay_Free", MoTa = "Tên Wifi", NhomCaiDat = "ChungChung" },
                new CaiDatHeThong { TenCaiDat = "MatKhauWifi", GiaTri = "homestay2026", MoTa = "Mật Khẩu Wifi", NhomCaiDat = "ChungChung" },
                new CaiDatHeThong { TenCaiDat = "MauChuDao", GiaTri = "#ff5a5f", MoTa = "Màu Chủ Đạo", NhomCaiDat = "GiaoDien" },
                new CaiDatHeThong { TenCaiDat = "MauPhu", GiaTri = "#00b4d8", MoTa = "Màu Phụ", NhomCaiDat = "GiaoDien" },
                new CaiDatHeThong { TenCaiDat = "LinkMessenger", GiaTri = "https://m.me/homestay", MoTa = "Link Messenger", NhomCaiDat = "LienHe" },
                new CaiDatHeThong { TenCaiDat = "LinkZalo", GiaTri = "https://zalo.me/0909123456", MoTa = "Link Zalo", NhomCaiDat = "LienHe" },
                new CaiDatHeThong { TenCaiDat = "BannerTrangChu", GiaTri = "Trải nghiệm homestay theo giờ – Tự do check-in, không cần chờ đợi!", MoTa = "Banner Trang Chủ", NhomCaiDat = "NoiDung" },
                new CaiDatHeThong { TenCaiDat = "NoiQuy", GiaTri = "1. Không hút thuốc trong phòng\n2. Giữ gìn vệ sinh chung\n3. Không gây ồn sau 22h\n4. Không mang vật nuôi\n5. Check-out đúng giờ", MoTa = "Nội Quy", NhomCaiDat = "NoiDung" },
                new CaiDatHeThong { TenCaiDat = "ChinhSach", GiaTri = "- Hủy trước 2 giờ: hoàn 100%\n- Hủy trước 1 giờ: hoàn 50%\n- Không đến: không hoàn tiền", MoTa = "Chính Sách Hủy", NhomCaiDat = "NoiDung" },
                new CaiDatHeThong { TenCaiDat = "ThoiGianGiuPhong", GiaTri = "30", MoTa = "Thời Gian Giữ Phòng (phút)", NhomCaiDat = "ChungChung" },
                new CaiDatHeThong { TenCaiDat = "BannerImages", GiaTri = "[]", MoTa = "Ảnh Hiển Thị Trang Chủ (tỉ lệ 21:9)", NhomCaiDat = "GiaoDien" },
                new CaiDatHeThong { TenCaiDat = "MaMoCua", GiaTri = "1234", MoTa = "Mã Mở Cửa (chung cho tất cả phòng)", NhomCaiDat = "ChungChung" },
                new CaiDatHeThong { TenCaiDat = "ThoiGianXoaDonHetHan", GiaTri = "30", MoTa = "Thời Gian Xóa Đơn Hết Hạn (ngày)", NhomCaiDat = "ChungChung" }
            };
            context.CaiDatHeThong.AddRange(caiDats);
            context.SaveChanges();

            // --- DatPhong mẫu ---
            var datPhongs = new DatPhong[]
            {
                new DatPhong { MaDatPhong = "DP-20260421-001", MaPhong = phongs[0].Id, TenKhach = "Nguyễn Văn A", SoDienThoai = "0901234567", Email = "nguyenvana@gmail.com", NgayDat = new DateTime(2026, 4, 21), NgayNhanPhong = new DateTime(2026, 4, 22), TrangThai = "DaXacNhan", TongTien = 450000, Token = "tok-abc123def456", MaMoCua = "1234", ThoiGianNhan = new DateTime(2026, 4, 22, 9, 0, 0), ThoiGianTra = new DateTime(2026, 4, 22, 15, 30, 0), NguonDat = "Web" },
                new DatPhong { MaDatPhong = "DP-20260421-002", MaPhong = phongs[2].Id, TenKhach = "Trần Thị B", SoDienThoai = "0912345678", NgayDat = new DateTime(2026, 4, 21), NgayNhanPhong = new DateTime(2026, 4, 22), TrangThai = "ChoXacNhan", TongTien = 900000, NguonDat = "Messenger" }
            };
            context.DatPhong.AddRange(datPhongs);
            context.SaveChanges();

            // --- ChiTietDatPhong ---
            var chiTiets = new ChiTietDatPhong[]
            {
                new ChiTietDatPhong { MaDatPhongId = datPhongs[0].Id, MaKhungGio = khungGios[0].Id, NgayApDung = new DateTime(2026, 4, 22), GiaApDung = 200000 },
                new ChiTietDatPhong { MaDatPhongId = datPhongs[0].Id, MaKhungGio = khungGios[1].Id, NgayApDung = new DateTime(2026, 4, 22), GiaApDung = 250000 },
                new ChiTietDatPhong { MaDatPhongId = datPhongs[1].Id, MaKhungGio = khungGios[3].Id, NgayApDung = new DateTime(2026, 4, 22), GiaApDung = 900000 }
            };
            context.ChiTietDatPhong.AddRange(chiTiets);
            context.SaveChanges();
        }
    }
}

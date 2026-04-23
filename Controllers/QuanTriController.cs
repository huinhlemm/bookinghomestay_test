using Booking_Homestay.Data;
using Booking_Homestay.Models;
using Booking_Homestay.Models.ViewModels;
using Booking_Homestay.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace Booking_Homestay.Controllers
{
    public class QuanTriController : Controller
    {
        private readonly UngDungDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public QuanTriController(UngDungDbContext db, IEmailService emailService, IWebHostEnvironment env)
        {
            _db = db;
            _emailService = emailService;
            _env = env;
        }

        private bool KiemTraDangNhap()
        {
            return HttpContext.Session.GetString("AdminDangNhap") == "true";
        }

        // ===== DASHBOARD =====
        public async Task<IActionResult> Index()
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");

            var homNay = DateTime.Today;
            var dauThang = new DateTime(homNay.Year, homNay.Month, 1);

            var viewModel = new QuanTriDashboardViewModel
            {
                TongDatPhong = await _db.DatPhong.CountAsync(),
                DatPhongHomNay = await _db.DatPhong.CountAsync(d => d.NgayDat.Date == homNay),
                DoanhThu = await _db.DatPhong.Where(d => d.TrangThai != "DaHuy").SumAsync(d => d.TongTien),
                DoanhThuThang = await _db.DatPhong
                    .Where(d => d.NgayDat >= dauThang && d.TrangThai != "DaHuy")
                    .SumAsync(d => d.TongTien),
                TongPhong = await _db.Phong.CountAsync(),
                PhongDangSuDung = await _db.DatPhong
                    .CountAsync(d => d.TrangThai == "DaNhanPhong"),
                ChoXacNhan = await _db.DatPhong.CountAsync(d => d.TrangThai == "ChoXacNhan"),
                DatPhongGanDay = await _db.DatPhong
                    .Include(d => d.Phong)
                    .OrderByDescending(d => d.NgayDat)
                    .Take(10)
                    .ToListAsync()
            };

            // Revenue last 7 days
            for (int i = 6; i >= 0; i--)
            {
                var ngay = homNay.AddDays(-i);
                var dt = await _db.DatPhong
                    .Where(d => d.NgayDat.Date == ngay && d.TrangThai != "DaHuy")
                    .SumAsync(d => d.TongTien);
                viewModel.DoanhThuTheoNgay[ngay.ToString("dd/MM")] = dt;
            }

            return View(viewModel);
        }

        // ===== PHÒNG =====
        public async Task<IActionResult> DanhSachPhong()
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");
            var phongs = await _db.Phong.Include(p => p.LoaiPhong).Include(p => p.KhuVuc).ToListAsync();
            ViewBag.LoaiPhongs = await _db.LoaiPhong.ToListAsync();
            ViewBag.KhuVucs = await _db.KhuVuc.ToListAsync();
            ViewBag.KhungGios = await _db.KhungGio.OrderBy(k => k.ThuTu).ToListAsync();
            return View(phongs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemPhong(Phong phong, List<decimal> giaTheoKhung)
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");

            _db.Phong.Add(phong);
            await _db.SaveChangesAsync();

            var khungGios = await _db.KhungGio.OrderBy(k => k.ThuTu).ToListAsync();
            for (int i = 0; i < khungGios.Count && i < giaTheoKhung.Count; i++)
            {
                _db.GiaPhong.Add(new GiaPhong
                {
                    MaPhong = phong.Id,
                    MaKhungGio = khungGios[i].Id,
                    Gia = giaTheoKhung[i]
                });
            }
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Thêm phòng thành công!";
            return RedirectToAction("DanhSachPhong");
        }

        [HttpGet]
        public async Task<IActionResult> LayPhong(int id)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });
            var p = await _db.Phong.FindAsync(id);
            if (p == null) return Json(new { success = false });
            return Json(new { success = true, phong = new {
                p.Id, p.TenPhong, p.MoTa, p.HinhAnh, p.DienTich, p.SoNguoiToiDa,
                p.DiaChi, p.TienNghi, p.MaLoaiPhong, p.MaKhuVuc, p.TrangThai
            }});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaPhong(Phong phong)
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");

            var existing = await _db.Phong.FindAsync(phong.Id);
            if (existing == null) return NotFound();

            existing.TenPhong = phong.TenPhong;
            existing.MoTa = phong.MoTa;
            existing.HinhAnh = phong.HinhAnh;
            existing.DienTich = phong.DienTich;
            existing.SoNguoiToiDa = phong.SoNguoiToiDa;
            existing.DiaChi = phong.DiaChi;
            existing.TienNghi = phong.TienNghi;
            existing.MaLoaiPhong = phong.MaLoaiPhong;
            existing.MaKhuVuc = phong.MaKhuVuc;
            existing.TrangThai = phong.TrangThai;

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = "Cập nhật phòng thành công!";
            return RedirectToAction("DanhSachPhong");
        }

        [HttpPost]
        public async Task<IActionResult> XoaPhong(int id)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });

            var phong = await _db.Phong.FindAsync(id);
            if (phong == null) return Json(new { success = false });

            var gias = _db.GiaPhong.Where(g => g.MaPhong == id);
            _db.GiaPhong.RemoveRange(gias);
            _db.Phong.Remove(phong);
            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ===== KHUNG GIỜ =====
        public async Task<IActionResult> DanhSachKhungGio()
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");
            return View(await _db.KhungGio.OrderBy(k => k.ThuTu).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemKhungGio(KhungGio khungGio)
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");
            _db.KhungGio.Add(khungGio);
            await _db.SaveChangesAsync();

            // Tự động sắp xếp thứ tự theo giờ bắt đầu (sáng → tối)
            var tatCaKhungGio = await _db.KhungGio.OrderBy(k => k.GioBatDau).ToListAsync();
            for (int i = 0; i < tatCaKhungGio.Count; i++)
            {
                tatCaKhungGio[i].ThuTu = i + 1;
            }
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Thêm khung giờ thành công!";
            return RedirectToAction("DanhSachKhungGio");
        }

        [HttpPost]
        public async Task<IActionResult> XoaKhungGio(int id)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });
            var kg = await _db.KhungGio.FindAsync(id);
            if (kg == null) return Json(new { success = false });
            _db.KhungGio.Remove(kg);
            await _db.SaveChangesAsync();

            // Tự động sắp xếp lại thứ tự
            var tatCaKhungGio = await _db.KhungGio.OrderBy(k => k.GioBatDau).ToListAsync();
            for (int i = 0; i < tatCaKhungGio.Count; i++)
            {
                tatCaKhungGio[i].ThuTu = i + 1;
            }
            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ===== ĐẶT PHÒNG =====
        public async Task<IActionResult> DanhSachDatPhong(string? trangThai)
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");

            var query = _db.DatPhong.Include(d => d.Phong).AsQueryable();
            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            ViewBag.TrangThai = trangThai;
            return View(await query.OrderByDescending(d => d.NgayDat).ToListAsync());
        }

        public async Task<IActionResult> ChiTietDatPhong(int id)
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");

            var datPhong = await _db.DatPhong
                .Include(d => d.Phong).ThenInclude(p => p!.KhuVuc)
                .Include(d => d.DanhSachChiTiet!).ThenInclude(ct => ct.KhungGio)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (datPhong == null) return NotFound();

            var caiDat = await _db.CaiDatHeThong.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            var qrUrl = $"{Request.Scheme}://{Request.Host}/DatPhong/XemVe/{datPhong.MaDatPhong}?token={datPhong.Token}";

            ViewBag.QrCodeBase64 = TaoQrCode(qrUrl);
            ViewBag.CaiDat = caiDat;
            return View(datPhong);
        }

        public async Task<IActionResult> TaoDatPhong()
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");
            ViewBag.Phongs = await _db.Phong.Where(p => p.TrangThai).ToListAsync();
            ViewBag.KhungGios = await _db.KhungGio.OrderBy(k => k.ThuTu).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoDatPhongXuLy(string tenKhach, string soDienThoai, string email, int maPhong, 
            List<int> danhSachKhungGioId, DateTime ngayNhanPhong, string? ghiChu, string nguonDat = "Web")
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");

            var giaPhongs = await _db.GiaPhong.Where(g => g.MaPhong == maPhong).ToListAsync();
            var khungGios = await _db.KhungGio.Where(k => danhSachKhungGioId.Contains(k.Id)).OrderBy(k => k.ThuTu).ToListAsync();

            var soThuTu = await _db.DatPhong.CountAsync() + 1;
            var maDatPhong = $"DP-{DateTime.Now:yyyyMMdd}-{soThuTu:D3}";
            var token = Guid.NewGuid().ToString("N")[..16];
            var maMoCua = new Random().Next(1000, 9999).ToString();

            decimal tongTien = 0;
            var chiTiets = new List<ChiTietDatPhong>();

            foreach (var kgId in danhSachKhungGioId)
            {
                var gia = giaPhongs.FirstOrDefault(g => g.MaKhungGio == kgId)?.Gia ?? 0;
                tongTien += gia;
                chiTiets.Add(new ChiTietDatPhong
                {
                    MaKhungGio = kgId,
                    NgayApDung = ngayNhanPhong.Date,
                    GiaApDung = gia
                });
            }

            var gioBatDau = khungGios.Min(k => k.GioBatDau);
            var gioKetThuc = khungGios.Max(k => k.GioKetThuc);

            var datPhong = new DatPhong
            {
                MaDatPhong = maDatPhong,
                MaPhong = maPhong,
                TenKhach = tenKhach,
                SoDienThoai = soDienThoai,
                Email = email,
                NgayDat = DateTime.Now,
                NgayNhanPhong = ngayNhanPhong.Date,
                TrangThai = "ChoXacNhan",
                TongTien = tongTien,
                GhiChu = ghiChu,
                Token = token,
                MaMoCua = maMoCua,
                ThoiGianNhan = ngayNhanPhong.Date + gioBatDau,
                ThoiGianTra = (gioKetThuc < gioBatDau) 
                    ? ngayNhanPhong.Date.AddDays(1) + gioKetThuc 
                    : ngayNhanPhong.Date + gioKetThuc,
                NguonDat = nguonDat,
                DanhSachChiTiet = chiTiets
            };

            _db.DatPhong.Add(datPhong);
            await _db.SaveChangesAsync();

            // Gửi email vé đặt phòng
            var phong = await _db.Phong.FindAsync(maPhong);
            var khungGioStrs = khungGios.Select(k => $"{k.BieuTuong} {k.TenKhungGio} ({k.GioBatDau:hh\\:mm}–{k.GioKetThuc:hh\\:mm})").ToList();
            var thanhToanUrl = $"{Request.Scheme}://{Request.Host}/DatPhong/ThanhCong?maDatPhong={datPhong.MaDatPhong}";
            await _emailService.GuiEmailDatPhong(datPhong, phong?.TenPhong ?? "", phong?.DiaChi ?? "", khungGioStrs, thanhToanUrl);

            TempData["ThongBao"] = $"Tạo đặt phòng thành công! Mã: {maDatPhong}";
            return RedirectToAction("ChiTietDatPhong", new { id = datPhong.Id });
        }

        [HttpPost]
        public async Task<IActionResult> XacNhanDatPhong(int id)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });

            var datPhong = await _db.DatPhong
                .Include(d => d.Phong)
                .Include(d => d.DanhSachChiTiet!).ThenInclude(ct => ct.KhungGio)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (datPhong == null) return Json(new { success = false });

            datPhong.TrangThai = "DaXacNhan";
            if (string.IsNullOrEmpty(datPhong.Token))
                datPhong.Token = Guid.NewGuid().ToString("N")[..16];
            if (string.IsNullOrEmpty(datPhong.MaMoCua))
                datPhong.MaMoCua = new Random().Next(1000, 9999).ToString();

            await _db.SaveChangesAsync();

            // Gửi email xác nhận
            var khungGioStrs = datPhong.DanhSachChiTiet?
                .Select(ct => $"{ct.KhungGio?.BieuTuong} {ct.KhungGio?.TenKhungGio} - {ct.GiaApDung:N0}đ")
                .ToList() ?? new List<string>();
            await _emailService.GuiEmailXacNhan(datPhong, datPhong.Phong?.TenPhong ?? "", datPhong.Phong?.DiaChi ?? "", khungGioStrs);

            return Json(new { success = true, maMoCua = datPhong.MaMoCua });
        }

        [HttpPost]
        public async Task<IActionResult> HuyDatPhong(int id)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });

            var datPhong = await _db.DatPhong.FindAsync(id);
            if (datPhong == null) return Json(new { success = false });

            datPhong.TrangThai = "DaHuy";
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DoiMaMoCua(int id, string maMoCua)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });

            var datPhong = await _db.DatPhong.FindAsync(id);
            if (datPhong == null) return Json(new { success = false });

            datPhong.MaMoCua = maMoCua;
            await _db.SaveChangesAsync();
            return Json(new { success = true, maMoCua = datPhong.MaMoCua });
        }

        // ===== KHU VỰC =====
        public async Task<IActionResult> DanhSachKhuVuc()
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");
            return View(await _db.KhuVuc.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemKhuVuc(KhuVuc khuVuc)
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");
            _db.KhuVuc.Add(khuVuc);
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = "Thêm khu vực thành công!";
            return RedirectToAction("DanhSachKhuVuc");
        }

        [HttpPost]
        public async Task<IActionResult> XoaKhuVuc(int id)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });
            var kv = await _db.KhuVuc.FindAsync(id);
            if (kv == null) return Json(new { success = false });
            _db.KhuVuc.Remove(kv);
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ===== GIÁ & KHUYẾN MÃI =====
        public async Task<IActionResult> GiaVaKhuyenMai()
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");
            ViewBag.GiaPhongs = await _db.GiaPhong.Include(g => g.Phong).Include(g => g.KhungGio).ToListAsync();
            ViewBag.KhungGios = await _db.KhungGio.OrderBy(k => k.ThuTu).ToListAsync();
            return View(await _db.KhuyenMai.Include(k => k.KhungGio).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemKhuyenMai(KhuyenMai khuyenMai)
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");
            _db.KhuyenMai.Add(khuyenMai);
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = "Thêm khuyến mãi thành công!";
            return RedirectToAction("GiaVaKhuyenMai");
        }

        // ===== CÀI ĐẶT =====
        public async Task<IActionResult> CaiDat()
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");
            
            // Auto-create BannerImages if not exists
            if (!await _db.CaiDatHeThong.AnyAsync(c => c.TenCaiDat == "BannerImages"))
            {
                _db.CaiDatHeThong.Add(new CaiDatHeThong { TenCaiDat = "BannerImages", GiaTri = "[]", MoTa = "Ảnh Hiển Thị Trang Chủ (tỉ lệ 21:9)", NhomCaiDat = "GiaoDien" });
                await _db.SaveChangesAsync();
            }

            // Auto-update MoTa display names
            var moTaMap = new Dictionary<string, string> {
                {"TenWebsite","Tên Website"},{"Logo","Logo Website"},{"Hotline","Số Điện Thoại"},
                {"Email","Email Liên Hệ"},{"DiaChi","Địa Chỉ"},{"TenWifi","Tên Wifi"},
                {"MatKhauWifi","Mật Khẩu Wifi"},{"MauChuDao","Màu Chủ Đạo"},{"MauPhu","Màu Phụ"},
                {"LinkMessenger","Link Messenger"},{"LinkZalo","Link Zalo"},
                {"BannerTrangChu","Banner Trang Chủ"},{"NoiQuy","Nội Quy"},
                {"ChinhSach","Chính Sách Hủy"},{"ThoiGianGiuPhong","Thời Gian Giữ Phòng (phút)"},
                {"BannerImages","Ảnh Hiển Thị Trang Chủ (tỉ lệ 21:9)"},
                {"QrThanhToan","Mã QR Thanh Toán"},{"HinhNen","Hình Nền"},
                {"ThongTinChuyenKhoan","Thông Tin Chuyển Khoản"},{"AdminPassword","Mật Khẩu Admin"},
                {"MaMoCua","Mã Mở Cửa (chung cho tất cả phòng)"},
                {"ThoiGianXoaDonHetHan","Thời Gian Xóa Đơn Hết Hạn (ngày)"}
            };
            var allSettings = await _db.CaiDatHeThong.ToListAsync();
            bool changed = false;
            foreach (var s in allSettings)
            {
                if (moTaMap.TryGetValue(s.TenCaiDat, out var newMoTa) && s.MoTa != newMoTa)
                {
                    s.MoTa = newMoTa;
                    changed = true;
                }
            }
            // Auto-create missing settings
            var autoCreate = new Dictionary<string, (string val, string nhom)> {
                {"MaMoCua", ("1234", "ChungChung")},
                {"ThoiGianXoaDonHetHan", ("30", "ChungChung")}
            };
            foreach (var kv in autoCreate)
            {
                if (!allSettings.Any(s => s.TenCaiDat == kv.Key))
                {
                    _db.CaiDatHeThong.Add(new CaiDatHeThong { TenCaiDat = kv.Key, GiaTri = kv.Value.val, MoTa = moTaMap[kv.Key], NhomCaiDat = kv.Value.nhom });
                    changed = true;
                }
            }
            if (changed) await _db.SaveChangesAsync();
            
            return View(allSettings.OrderBy(c => c.NhomCaiDat).ThenBy(c => c.Id).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LuuCaiDat(Dictionary<int, string> caiDatValues)
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");

            foreach (var item in caiDatValues)
            {
                var caiDat = await _db.CaiDatHeThong.FindAsync(item.Key);
                if (caiDat != null)
                {
                    caiDat.GiaTri = item.Value;
                }
            }
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = "Lưu cài đặt thành công!";
            return RedirectToAction("CaiDat");
        }

        // ===== QUẢN LÝ TÀI KHOẢN =====
        public async Task<IActionResult> QuanLyTaiKhoan()
        {
            if (!KiemTraDangNhap()) return RedirectToAction("DangNhap", "TaiKhoan");
            var taiKhoans = await _db.TaiKhoan.OrderByDescending(t => t.NgayTao).ToListAsync();
            return View(taiKhoans);
        }

        [HttpPost]
        public async Task<IActionResult> DuyetTaiKhoan(int id)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });
            var tk = await _db.TaiKhoan.FindAsync(id);
            if (tk == null) return Json(new { success = false });
            tk.DaDuyet = true;
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> KhoaTaiKhoan(int id)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });
            var tk = await _db.TaiKhoan.FindAsync(id);
            if (tk == null) return Json(new { success = false });
            tk.BiKhoa = !tk.BiKhoa;
            await _db.SaveChangesAsync();
            return Json(new { success = true, biKhoa = tk.BiKhoa });
        }

        [HttpPost]
        public async Task<IActionResult> XoaTaiKhoan(int id)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });
            var tk = await _db.TaiKhoan.FindAsync(id);
            if (tk == null) return Json(new { success = false });
            if (tk.TenDangNhap == "admin") return Json(new { success = false, message = "Không thể xóa tài khoản admin gốc!" });
            _db.TaiKhoan.Remove(tk);
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        private string TaoQrCode(string url)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(10);
            return Convert.ToBase64String(qrCodeBytes);
        }

        [HttpPost]
        public async Task<IActionResult> UploadHinhCaiDat(int id, IFormFile file)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file" });

            var caiDat = await _db.CaiDatHeThong.FindAsync(id);
            if (caiDat == null) return Json(new { success = false });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "settings");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            caiDat.GiaTri = "/uploads/settings/" + uniqueFileName;
            await _db.SaveChangesAsync();

            return Json(new { success = true, filePath = caiDat.GiaTri });
        }

        [HttpPost]
        public async Task<IActionResult> UploadHinhAnh(IFormFile file)
        {
            if (!KiemTraDangNhap()) return Json(new { success = false });
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file" });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "images");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Json(new { success = true, filePath = "/uploads/images/" + uniqueFileName });
        }
    }
}

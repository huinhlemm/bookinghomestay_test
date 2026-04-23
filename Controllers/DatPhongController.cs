using Booking_Homestay.Data;
using Booking_Homestay.Models;
using Booking_Homestay.Models.ViewModels;
using Booking_Homestay.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace Booking_Homestay.Controllers
{
    public class DatPhongController : Controller
    {
        private readonly UngDungDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public DatPhongController(UngDungDbContext db, IEmailService emailService, IWebHostEnvironment env)
        {
            _db = db;
            _emailService = emailService;
            _env = env;
        }

        public async Task<IActionResult> Index(int maPhong, string? ngay)
        {
            await AutoHuyDatPhongHetHan();

            var phong = await _db.Phong
                .Include(p => p.LoaiPhong)
                .Include(p => p.KhuVuc)
                .Include(p => p.DanhSachGia!)
                    .ThenInclude(g => g.KhungGio)
                .FirstOrDefaultAsync(p => p.Id == maPhong);

            if (phong == null) return NotFound();

            var ngayDat = string.IsNullOrEmpty(ngay) ? DateTime.Today : DateTime.Parse(ngay);
            var caiDat = await _db.CaiDatHeThong.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);

            // Get booked slots for the date
            var daDat = await _db.ChiTietDatPhong
                .Include(ct => ct.DatPhong)
                .Where(ct => ct.NgayApDung.Date == ngayDat.Date &&
                    ct.DatPhong!.MaPhong == maPhong &&
                    ct.DatPhong.TrangThai != "DaHuy")
                .Select(ct => ct.MaKhungGio)
                .ToListAsync();

            var khungGios = await _db.KhungGio.OrderBy(k => k.ThuTu).ToListAsync();
            var giaPhongs = phong.DanhSachGia?.ToList() ?? new();

            var danhSachKhungGio = khungGios.Select(kg => new KhungGioTrangThai
            {
                KhungGio = kg,
                Gia = giaPhongs.FirstOrDefault(g => g.MaKhungGio == kg.Id)?.Gia ?? 0,
                GiaGoc = giaPhongs.FirstOrDefault(g => g.MaKhungGio == kg.Id)?.GiaGoc,
                DaDat = daDat.Contains(kg.Id),
                DangChon = false
            }).ToList();

            var viewModel = new DatPhongViewModel
            {
                MaPhong = maPhong,
                TenPhong = phong.TenPhong,
                NgayNhanPhong = ngayDat,
                ThongTinPhong = phong,
                DanhSachKhungGio = danhSachKhungGio,
                CaiDat = caiDat!
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> KiemTraKhungGio(int maPhong, string ngay)
        {
            await AutoHuyDatPhongHetHan();

            var ngayDat = DateTime.Parse(ngay);
            var daDat = await _db.ChiTietDatPhong
                .Include(ct => ct.DatPhong)
                .Where(ct => ct.NgayApDung.Date == ngayDat.Date &&
                    ct.DatPhong!.MaPhong == maPhong &&
                    ct.DatPhong.TrangThai != "DaHuy")
                .Select(ct => ct.MaKhungGio)
                .ToListAsync();

            var giaPhongs = await _db.GiaPhong
                .Where(g => g.MaPhong == maPhong)
                .ToListAsync();

            var khungGios = await _db.KhungGio.OrderBy(k => k.ThuTu).ToListAsync();

            var result = khungGios.Select(kg => new
            {
                id = kg.Id,
                ten = kg.TenKhungGio,
                gioBatDau = kg.GioBatDau.ToString(@"hh\:mm"),
                gioKetThuc = kg.GioKetThuc.ToString(@"hh\:mm"),
                bieuTuong = kg.BieuTuong,
                gia = giaPhongs.FirstOrDefault(g => g.MaKhungGio == kg.Id)?.Gia ?? 0,
                daDat = daDat.Contains(kg.Id)
            });

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoDatPhong(DatPhongViewModel model)
        {
            await AutoHuyDatPhongHetHan();

            if (model.DanhSachKhungGioId == null || !model.DanhSachKhungGioId.Any())
            {
                TempData["Loi"] = "Vui lòng chọn ít nhất 1 khung giờ!";
                return RedirectToAction("Index", new { maPhong = model.MaPhong, ngay = model.NgayNhanPhong.ToString("yyyy-MM-dd") });
            }

            // Check conflicts
            var daDat = await _db.ChiTietDatPhong
                .Include(ct => ct.DatPhong)
                .Where(ct => ct.NgayApDung.Date == model.NgayNhanPhong.Date &&
                    ct.DatPhong!.MaPhong == model.MaPhong &&
                    ct.DatPhong.TrangThai != "DaHuy" &&
                    model.DanhSachKhungGioId.Contains(ct.MaKhungGio))
                .AnyAsync();

            if (daDat)
            {
                TempData["Loi"] = "Khung giờ đã được đặt, vui lòng chọn khung giờ khác!";
                return RedirectToAction("Index", new { maPhong = model.MaPhong, ngay = model.NgayNhanPhong.ToString("yyyy-MM-dd") });
            }

            // Get pricing
            var giaPhongs = await _db.GiaPhong
                .Where(g => g.MaPhong == model.MaPhong)
                .ToListAsync();

            var khungGios = await _db.KhungGio
                .Where(k => model.DanhSachKhungGioId.Contains(k.Id))
                .OrderBy(k => k.ThuTu)
                .ToListAsync();

            // Generate booking code
            var soThuTu = await _db.DatPhong.CountAsync() + 1;
            var maDatPhong = $"DP-{DateTime.Now:yyyyMMdd}-{soThuTu:D3}";
            var token = Guid.NewGuid().ToString("N")[..16];
            var maMoCuaSetting = await _db.CaiDatHeThong.FirstOrDefaultAsync(c => c.TenCaiDat == "MaMoCua");
            var maMoCua = maMoCuaSetting?.GiaTri ?? "1234";

            // Calculate time range
            var gioBatDau = khungGios.Min(k => k.GioBatDau);
            var gioKetThuc = khungGios.Max(k => k.GioKetThuc);
            if (gioKetThuc < gioBatDau) // Qua đêm
                gioKetThuc = new TimeSpan(8, 0, 0);

            decimal tongTien = 0;
            var chiTiets = new List<ChiTietDatPhong>();

            foreach (var kgId in model.DanhSachKhungGioId)
            {
                var gia = giaPhongs.FirstOrDefault(g => g.MaKhungGio == kgId)?.Gia ?? 0;
                tongTien += gia;
                chiTiets.Add(new ChiTietDatPhong
                {
                    MaKhungGio = kgId,
                    NgayApDung = model.NgayNhanPhong.Date,
                    GiaApDung = gia
                });
            }

            var datPhong = new DatPhong
            {
                MaDatPhong = maDatPhong,
                MaPhong = model.MaPhong,
                TenKhach = model.TenKhach,
                SoDienThoai = model.SoDienThoai,
                Email = model.Email,
                NgayDat = DateTime.Now,
                NgayNhanPhong = model.NgayNhanPhong.Date,
                TrangThai = "ChoXacNhan",
                TongTien = tongTien,
                GhiChu = model.GhiChu,
                Token = token,
                MaMoCua = maMoCua,
                ThoiGianNhan = model.NgayNhanPhong.Date + gioBatDau,
                ThoiGianTra = (gioKetThuc.Hours < gioBatDau.Hours)
                    ? model.NgayNhanPhong.Date.AddDays(1) + gioKetThuc
                    : model.NgayNhanPhong.Date + gioKetThuc,
                NguonDat = model.NguonDat,
                CccdMatTruoc = model.CccdMatTruoc,
                CccdMatSau = model.CccdMatSau,
                DanhSachChiTiet = chiTiets
            };

            _db.DatPhong.Add(datPhong);
            await _db.SaveChangesAsync();

            // Gửi email vé đặt phòng
            var phong = await _db.Phong.FindAsync(model.MaPhong);
            var khungGioStrs = khungGios.Select(k => $"{k.BieuTuong} {k.TenKhungGio} ({k.GioBatDau:hh\\:mm}–{k.GioKetThuc:hh\\:mm})").ToList();
            var thanhToanUrl = $"{Request.Scheme}://{Request.Host}/DatPhong/ThanhCong?maDatPhong={datPhong.MaDatPhong}";
            await _emailService.GuiEmailDatPhong(datPhong, phong?.TenPhong ?? "", phong?.DiaChi ?? "", khungGioStrs, thanhToanUrl);

            // Gửi email thông báo cho tất cả user (admin + nhân viên)
            var allUserEmails = await _db.TaiKhoan
                .Where(u => !string.IsNullOrEmpty(u.Email) && u.DaDuyet && !u.BiKhoa)
                .Select(u => u.Email!)
                .ToListAsync();
            // Thêm email admin từ cài đặt hệ thống
            var adminEmail = await _db.CaiDatHeThong
                .Where(c => c.TenCaiDat == "Email")
                .Select(c => c.GiaTri)
                .FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(adminEmail) && !allUserEmails.Contains(adminEmail))
                allUserEmails.Add(adminEmail);
            await _emailService.GuiEmailThongBaoDatPhong(allUserEmails, datPhong, phong?.TenPhong ?? "", khungGioStrs);

            return RedirectToAction("ThanhCong", new { maDatPhong = datPhong.MaDatPhong });
        }

        public async Task<IActionResult> ThanhCong(string maDatPhong)
        {
            var datPhong = await _db.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.KhuVuc)
                .Include(d => d.DanhSachChiTiet!)
                    .ThenInclude(ct => ct.KhungGio)
                .FirstOrDefaultAsync(d => d.MaDatPhong == maDatPhong);

            if (datPhong == null) return NotFound();

            var caiDat = await _db.CaiDatHeThong.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            var qrUrl = $"{Request.Scheme}://{Request.Host}/DatPhong/XemVe/{datPhong.MaDatPhong}?token={datPhong.Token}";
            var qrBase64 = TaoQrCode(qrUrl);

            var viewModel = new VeDatPhongViewModel
            {
                DatPhong = datPhong,
                Phong = datPhong.Phong!,
                DanhSachChiTiet = datPhong.DanhSachChiTiet?.ToList() ?? new(),
                QrCodeBase64 = qrBase64,
                HopLe = true,
                CaiDat = caiDat!
            };

            return View(viewModel);
        }

        [Route("DatPhong/XemVe/{ma}")]
        public async Task<IActionResult> XemVe(string ma, string? token)
        {
            var datPhong = await _db.DatPhong
                .Include(d => d.Phong)
                    .ThenInclude(p => p!.KhuVuc)
                .Include(d => d.DanhSachChiTiet!)
                    .ThenInclude(ct => ct.KhungGio)
                .FirstOrDefaultAsync(d => d.MaDatPhong == ma);

            if (datPhong == null)
                return View(new VeDatPhongViewModel { HopLe = false, LoiThongBao = "Không tìm thấy đặt phòng!" });

            var caiDat = await _db.CaiDatHeThong.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);

            // Security check
            bool hopLe = true;
            string? loi = null;

            if (string.IsNullOrEmpty(token) || token != datPhong.Token)
            {
                hopLe = false;
                loi = "Token không hợp lệ hoặc đã hết hạn!";
            }
            else if (datPhong.TrangThai == "DaHuy")
            {
                hopLe = false;
                loi = "Đặt phòng đã bị hủy!";
            }
            else if (datPhong.ThoiGianTra.HasValue && DateTime.Now > datPhong.ThoiGianTra.Value.AddHours(1))
            {
                hopLe = false;
                loi = "Đặt phòng đã hết thời gian sử dụng!";
            }

            var qrUrl = $"{Request.Scheme}://{Request.Host}/DatPhong/XemVe/{datPhong.MaDatPhong}?token={datPhong.Token}";

            var viewModel = new VeDatPhongViewModel
            {
                DatPhong = datPhong,
                Phong = datPhong.Phong!,
                DanhSachChiTiet = datPhong.DanhSachChiTiet?.ToList() ?? new(),
                QrCodeBase64 = TaoQrCode(qrUrl),
                HopLe = hopLe,
                LoiThongBao = loi,
                CaiDat = caiDat!
            };

            return View(viewModel);
        }

        public async Task<IActionResult> TraCuu(string? ma)
        {
            if (string.IsNullOrEmpty(ma)) return View();

            var datPhong = await _db.DatPhong
                .Include(d => d.Phong)
                .Include(d => d.DanhSachChiTiet!)
                    .ThenInclude(ct => ct.KhungGio)
                .FirstOrDefaultAsync(d => d.MaDatPhong == ma || d.SoDienThoai == ma);

            ViewBag.KetQua = datPhong;
            ViewBag.Ma = ma;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> TaoNoiDungChat(int maPhong, string ngay, string khungGio, string tenKhach, string sdt)
        {
            var phong = await _db.Phong.FindAsync(maPhong);
            if (phong == null) return Json(new { success = false });

            var noiDung = $"Xin chào, tôi muốn đặt phòng:\n" +
                $"🏠 Phòng: {phong.TenPhong}\n" +
                $"📅 Ngày: {ngay}\n" +
                $"⏰ Khung giờ: {khungGio}\n" +
                $"👤 Tên: {tenKhach}\n" +
                $"📱 SĐT: {sdt}";

            return Json(new { success = true, noiDung = noiDung });
        }

        [HttpPost]
        public async Task<IActionResult> UploadMinhChung(string maDatPhong, IFormFile billFile)
        {
            if (billFile == null || billFile.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn file" });

            var datPhong = await _db.DatPhong.FirstOrDefaultAsync(d => d.MaDatPhong == maDatPhong);
            if (datPhong == null)
                return Json(new { success = false, message = "Không tìm thấy đơn đặt phòng" });

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "bills");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(billFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await billFile.CopyToAsync(fileStream);
            }

            datPhong.MinhChungThanhToan = "/uploads/bills/" + uniqueFileName;
            await _db.SaveChangesAsync();

            return Json(new { success = true, filePath = datPhong.MinhChungThanhToan });
        }

        private string TaoQrCode(string url)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(10);
            return Convert.ToBase64String(qrCodeBytes);
        }

        private byte[] TaoQrCodeBytes(string url)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }

        [Route("DatPhong/TaiVePDF/{ma}")]
        public async Task<IActionResult> TaiVePDF(string ma, string? token)
        {
            var datPhong = await _db.DatPhong
                .Include(d => d.Phong)
                .Include(d => d.DanhSachChiTiet!)
                    .ThenInclude(ct => ct.KhungGio)
                .FirstOrDefaultAsync(d => d.MaDatPhong == ma);

            if (datPhong == null) return NotFound();

            // Cho phép admin hoặc user có token hợp lệ
            var isAdmin = HttpContext.Session.GetInt32("UserId") != null;
            if (!isAdmin && (string.IsNullOrEmpty(token) || token != datPhong.Token))
                return Unauthorized("Token không hợp lệ!");

            var caiDat = await _db.CaiDatHeThong.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);
            var qrUrl = $"{Request.Scheme}://{Request.Host}/DatPhong/XemVe/{datPhong.MaDatPhong}?token={datPhong.Token}";
            var qrBytes = TaoQrCodeBytes(qrUrl);

            var trangThai = datPhong.TrangThai switch
            {
                "ChoXacNhan" => "Ch\u1edd x\u00e1c nh\u1eadn",
                "DaXacNhan" => "\u0110\u00e3 x\u00e1c nh\u1eadn",
                "DaNhanPhong" => "\u0110\u00e3 nh\u1eadn ph\u00f2ng",
                "HoanThanh" => "Ho\u00e0n th\u00e0nh",
                "DaHuy" => "\u0110\u00e3 h\u1ee7y",
                "DaHetHan" => "\u0110\u00e3 h\u1ebft h\u1ea1n",
                _ => datPhong.TrangThai ?? ""
            };

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(0);
                    page.DefaultTextStyle(x => x.FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        // ===== HEADER =====
                        col.Item().Background("#ff5a5f").Padding(25).Column(h =>
                        {
                            h.Item().AlignCenter().Text("HomeStay Booking")
                                .FontSize(24).Bold().FontColor("#ffffff");
                            h.Item().AlignCenter().PaddingTop(5)
                                .Text("V\u00c9 \u0110\u1eb6T PH\u00d2NG \u0110I\u1ec6N T\u1eed")
                                .FontSize(12).FontColor("#ffe0e0");
                        });

                        // ===== QR CODE =====
                        col.Item().Background("#fafafa").PaddingVertical(18).Column(qr =>
                        {
                            qr.Item().AlignCenter().Width(140).Image(qrBytes);
                            qr.Item().AlignCenter().PaddingTop(8)
                                .Text("Qu\u00e9t m\u00e3 QR \u0111\u1ec3 xem v\u00e9 online")
                                .FontSize(9).FontColor("#999999").Italic();
                        });

                        // ===== DASHED DIVIDER =====
                        col.Item().PaddingHorizontal(20).LineHorizontal(1).LineColor("#dddddd");

                        // ===== BOOKING INFO =====
                        col.Item().PaddingHorizontal(20).PaddingVertical(8).Column(info =>
                        {
                            info.Item().PaddingBottom(8).Text("TH\u00d4NG TIN \u0110\u1eb6T PH\u00d2NG")
                                .FontSize(11).Bold().FontColor("#ff5a5f");

                            int rowIdx = 0;
                            void ThemDong(string nhan, string giaTri, bool noiB = false)
                            {
                                var bgColor = rowIdx % 2 == 0 ? "#ffffff" : "#f9f5f5";
                                info.Item().Background(bgColor).PaddingVertical(5).PaddingHorizontal(8).Row(row =>
                                {
                                    row.RelativeItem(2).Text(nhan).FontSize(10).FontColor("#888888");
                                    if (noiB)
                                        row.RelativeItem(3).AlignRight().Text(giaTri).FontSize(10).Bold().FontColor("#ff5a5f");
                                    else
                                        row.RelativeItem(3).AlignRight().Text(giaTri).FontSize(10).FontColor("#333333");
                                });
                                rowIdx++;
                            }

                            ThemDong("M\u00e3 \u0111\u1eb7t ph\u00f2ng", datPhong.MaDatPhong, true);
                            ThemDong("Kh\u00e1ch h\u00e0ng", datPhong.TenKhach);
                            ThemDong("S\u1ed1 \u0111i\u1ec7n tho\u1ea1i", datPhong.SoDienThoai);
                            if (!string.IsNullOrEmpty(datPhong.Email))
                                ThemDong("Email", datPhong.Email);
                            ThemDong("Ph\u00f2ng", datPhong.Phong?.TenPhong ?? "");
                            ThemDong("\u0110\u1ecba ch\u1ec9", datPhong.Phong?.DiaChi ?? "");
                            ThemDong("Ng\u00e0y nh\u1eadn", datPhong.NgayNhanPhong.ToString("dd/MM/yyyy"));
                            ThemDong("Nh\u1eadn ph\u00f2ng", datPhong.ThoiGianNhan?.ToString("HH:mm - dd/MM/yyyy") ?? "");
                            ThemDong("Tr\u1ea3 ph\u00f2ng", datPhong.ThoiGianTra?.ToString("HH:mm - dd/MM/yyyy") ?? "");
                            ThemDong("Ngu\u1ed3n \u0111\u1eb7t", datPhong.NguonDat ?? "Web");
                            ThemDong("Tr\u1ea1ng th\u00e1i", trangThai);

                            // Khung giờ
                            if (datPhong.DanhSachChiTiet?.Any() == true)
                            {
                                info.Item().PaddingTop(6).PaddingHorizontal(8).Text("Khung gi\u1edd:")
                                    .FontSize(10).FontColor("#888888");
                                foreach (var ct in datPhong.DanhSachChiTiet)
                                {
                                    var kgText = $"{ct.KhungGio?.TenKhungGio}  ({ct.KhungGio?.GioBatDau:hh\\:mm} - {ct.KhungGio?.GioKetThuc:hh\\:mm})  —  {ct.GiaApDung:N0}\u0111";
                                    info.Item().PaddingHorizontal(8).PaddingVertical(2)
                                        .Text(kgText).FontSize(10).FontColor("#e65100");
                                }
                            }
                        });

                        // ===== DOOR CODE =====
                        col.Item().PaddingHorizontal(20).PaddingTop(5).Element(e =>
                            e.Background("#667eea").Padding(16).Column(code =>
                            {
                                code.Item().AlignCenter().Text("M\u00c3 M\u1ede C\u1eeca")
                                    .FontSize(10).FontColor("#d0d8ff");
                                code.Item().AlignCenter().PaddingTop(4)
                                    .Text(datPhong.MaMoCua ?? "----")
                                    .FontSize(30).Bold().FontColor("#ffffff");
                            })
                        );

                        // ===== TOTAL =====
                        col.Item().PaddingHorizontal(20).PaddingTop(4).Element(e =>
                            e.Background("#ff5a5f").Padding(14).Row(total =>
                            {
                                total.RelativeItem().AlignLeft().AlignMiddle()
                                    .Text("T\u1ed5ng ti\u1ec1n").FontSize(13).Bold().FontColor("#ffffff");
                                total.RelativeItem().AlignRight().AlignMiddle()
                                    .Text($"{datPhong.TongTien:N0}\u0111").FontSize(20).Bold().FontColor("#ffffff");
                            })
                        );

                        // ===== WIFI & HOTLINE =====
                        var wifi = caiDat.GetValueOrDefault("TenWifi", "");
                        var wifiPass = caiDat.GetValueOrDefault("MatKhauWifi", "");
                        var hotline = caiDat.GetValueOrDefault("Hotline", "");
                        if (!string.IsNullOrEmpty(wifi) || !string.IsNullOrEmpty(hotline))
                        {
                            col.Item().PaddingHorizontal(20).PaddingTop(10)
                                .Background("#f5f5f5").Padding(12).Column(ft =>
                            {
                                if (!string.IsNullOrEmpty(wifi))
                                    ft.Item().Text($"WiFi: {wifi}  |  M\u1eadt kh\u1ea9u: {wifiPass}")
                                        .FontSize(10).FontColor("#555555");
                                if (!string.IsNullOrEmpty(hotline))
                                    ft.Item().PaddingTop(3).Text($"Hotline: {hotline}")
                                        .FontSize(10).FontColor("#555555");
                            });
                        }

                        // ===== FOOTER =====
                        col.Item().PaddingTop(12).AlignCenter()
                            .Text("C\u1ea3m \u01a1n qu\u00fd kh\u00e1ch \u0111\u00e3 s\u1eed d\u1ee5ng d\u1ecbch v\u1ee5!")
                            .FontSize(10).Italic().FontColor("#999999");
                        col.Item().PaddingTop(2).AlignCenter()
                            .Text("Xu\u1ea5t v\u00e9: " + DateTime.Now.ToString("HH:mm dd/MM/yyyy"))
                            .FontSize(8).FontColor("#cccccc");
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"ve-dat-phong-{ma}.pdf");
        }

        private async Task AutoHuyDatPhongHetHan()
        {
            // 1. Hủy đơn chờ xác nhận quá thời gian giữ phòng
            var strThoiGianGiu = await _db.CaiDatHeThong
                .Where(c => c.TenCaiDat == "ThoiGianGiuPhong")
                .Select(c => c.GiaTri)
                .FirstOrDefaultAsync();

            int thoiGianGiuPhong = 30;
            if (!string.IsNullOrEmpty(strThoiGianGiu) && int.TryParse(strThoiGianGiu, out int parsed))
                thoiGianGiuPhong = parsed;

            var limitTime = DateTime.Now.AddMinutes(-thoiGianGiuPhong);
            var expiredPending = await _db.DatPhong
                .Where(d => d.TrangThai == "ChoXacNhan" && d.NgayDat < limitTime)
                .ToListAsync();
            foreach (var b in expiredPending) b.TrangThai = "DaHuy";

            // 2. Đánh dấu hết hạn: đơn đã xác nhận/hoàn thành mà ThoiGianTra đã qua
            var now = DateTime.Now;
            var hetHan = await _db.DatPhong
                .Where(d => (d.TrangThai == "DaXacNhan" || d.TrangThai == "DaNhanPhong" || d.TrangThai == "HoanThanh")
                    && d.ThoiGianTra != null && d.ThoiGianTra < now)
                .ToListAsync();
            foreach (var b in hetHan) b.TrangThai = "DaHetHan";

            if (expiredPending.Any() || hetHan.Any())
                await _db.SaveChangesAsync();

            // 3. Xóa đơn hết hạn sau N ngày (admin setting), lưu doanh thu trước
            var strXoa = await _db.CaiDatHeThong
                .Where(c => c.TenCaiDat == "ThoiGianXoaDonHetHan")
                .Select(c => c.GiaTri)
                .FirstOrDefaultAsync();
            int ngayXoa = 30;
            if (!string.IsNullOrEmpty(strXoa) && int.TryParse(strXoa, out int parsedXoa))
                ngayXoa = parsedXoa;

            var hanXoa = now.AddDays(-ngayXoa);
            var donCanXoa = await _db.DatPhong
                .Include(d => d.DanhSachChiTiet)
                .Where(d => (d.TrangThai == "DaHetHan" || d.TrangThai == "DaHuy")
                    && d.ThoiGianTra != null && d.ThoiGianTra < hanXoa)
                .ToListAsync();

            if (donCanXoa.Any())
            {
                // Lưu doanh thu theo tháng trước khi xóa
                var grouped = donCanXoa
                    .Where(d => d.TrangThai == "DaHetHan" && d.TongTien > 0)
                    .GroupBy(d => new { d.NgayNhanPhong.Month, d.NgayNhanPhong.Year });

                foreach (var g in grouped)
                {
                    var existing = await _db.DoanhThu.FirstOrDefaultAsync(
                        dt => dt.Thang == g.Key.Month && dt.Nam == g.Key.Year);
                    if (existing != null)
                    {
                        existing.TongDoanhThu += g.Sum(d => d.TongTien);
                        existing.SoDon += g.Count();
                    }
                    else
                    {
                        _db.DoanhThu.Add(new DoanhThu
                        {
                            Thang = g.Key.Month,
                            Nam = g.Key.Year,
                            TongDoanhThu = g.Sum(d => d.TongTien),
                            SoDon = g.Count(),
                            GhiChu = $"Tự động lưu khi xóa đơn hết hạn"
                        });
                    }
                }

                _db.DatPhong.RemoveRange(donCanXoa);
                await _db.SaveChangesAsync();
            }
        }
    }
}

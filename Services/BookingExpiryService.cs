using Booking_Homestay.Data;
using Booking_Homestay.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking_Homestay.Services
{
    public class BookingExpiryService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingExpiryService> _logger;

        public BookingExpiryService(IServiceProvider serviceProvider, ILogger<BookingExpiryService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await KiemTraHetHan();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[BookingExpiry] Lỗi kiểm tra hết hạn");
                }

                // Chạy mỗi 1 phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task KiemTraHetHan()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<UngDungDbContext>();
            var now = DateTime.Now;
            bool changed = false;

            // 1. Hủy đơn chờ xác nhận quá thời gian giữ phòng
            var strThoiGianGiu = await db.CaiDatHeThong
                .Where(c => c.TenCaiDat == "ThoiGianGiuPhong")
                .Select(c => c.GiaTri)
                .FirstOrDefaultAsync();
            int thoiGianGiuPhong = 30;
            if (!string.IsNullOrEmpty(strThoiGianGiu) && int.TryParse(strThoiGianGiu, out int parsed))
                thoiGianGiuPhong = parsed;

            var limitTime = now.AddMinutes(-thoiGianGiuPhong);
            var expiredPending = await db.DatPhong
                .Where(d => d.TrangThai == "ChoXacNhan" && d.NgayDat < limitTime)
                .ToListAsync();
            foreach (var b in expiredPending) { b.TrangThai = "DaHuy"; changed = true; }

            // 2. Đánh dấu hết hạn: đơn mà ThoiGianTra đã qua
            var hetHan = await db.DatPhong
                .Where(d => (d.TrangThai == "DaXacNhan" || d.TrangThai == "DaNhanPhong" || d.TrangThai == "HoanThanh")
                    && d.ThoiGianTra != null && d.ThoiGianTra < now)
                .ToListAsync();
            foreach (var b in hetHan) { b.TrangThai = "DaHetHan"; changed = true; }

            if (changed) await db.SaveChangesAsync();

            // 3. Xóa đơn hết hạn/hủy sau N ngày, lưu doanh thu
            var strXoa = await db.CaiDatHeThong
                .Where(c => c.TenCaiDat == "ThoiGianXoaDonHetHan")
                .Select(c => c.GiaTri)
                .FirstOrDefaultAsync();
            int ngayXoa = 30;
            if (!string.IsNullOrEmpty(strXoa) && int.TryParse(strXoa, out int parsedXoa))
                ngayXoa = parsedXoa;

            var hanXoa = now.AddDays(-ngayXoa);
            var donCanXoa = await db.DatPhong
                .Include(d => d.DanhSachChiTiet)
                .Where(d => (d.TrangThai == "DaHetHan" || d.TrangThai == "DaHuy")
                    && d.ThoiGianTra != null && d.ThoiGianTra < hanXoa)
                .ToListAsync();

            if (donCanXoa.Any())
            {
                var grouped = donCanXoa
                    .Where(d => d.TrangThai == "DaHetHan" && d.TongTien > 0)
                    .GroupBy(d => new { d.NgayNhanPhong.Month, d.NgayNhanPhong.Year });

                foreach (var g in grouped)
                {
                    var existing = await db.DoanhThu.FirstOrDefaultAsync(
                        dt => dt.Thang == g.Key.Month && dt.Nam == g.Key.Year);
                    if (existing != null)
                    {
                        existing.TongDoanhThu += g.Sum(d => d.TongTien);
                        existing.SoDon += g.Count();
                    }
                    else
                    {
                        db.DoanhThu.Add(new DoanhThu
                        {
                            Thang = g.Key.Month,
                            Nam = g.Key.Year,
                            TongDoanhThu = g.Sum(d => d.TongTien),
                            SoDon = g.Count(),
                            GhiChu = "Tự động lưu khi xóa đơn hết hạn"
                        });
                    }
                }

                db.DatPhong.RemoveRange(donCanXoa);
                await db.SaveChangesAsync();
                _logger.LogInformation($"[BookingExpiry] Đã xóa {donCanXoa.Count} đơn hết hạn");
            }
        }
    }
}

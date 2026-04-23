using Booking_Homestay.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Booking_Homestay.Services
{
    public interface IEmailService
    {
        Task GuiEmailDatPhong(DatPhong datPhong, string tenPhong, string diaChi, List<string> khungGios, string thanhToanUrl);
        Task GuiEmailXacNhan(DatPhong datPhong, string tenPhong, string diaChi, List<string> khungGios);
        Task GuiEmailThongBaoDatPhong(List<string> danhSachEmail, DatPhong datPhong, string tenPhong, List<string> khungGios);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task GuiEmailDatPhong(DatPhong datPhong, string tenPhong, string diaChi, List<string> khungGios, string thanhToanUrl)
        {
            if (string.IsNullOrEmpty(datPhong.Email)) return;
            if (string.IsNullOrEmpty(_settings.SenderEmail)) return;

            var khungGioHtml = string.Join("", khungGios.Select(kg => $"<tr><td style='padding:8px 12px;border-bottom:1px solid #f0f0f0;'>{kg}</td></tr>"));

            var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;font-family:Arial,sans-serif;background:#f5f5f5;'>
<div style='max-width:600px;margin:20px auto;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.1);'>
    <div style='background:linear-gradient(135deg,#ff5a5f,#ff8a8e);padding:30px;text-align:center;color:white;'>
        <h1 style='margin:0;font-size:24px;'>🏠 HomeStay Booking</h1>
        <p style='margin:8px 0 0;opacity:0.9;'>Xác nhận đặt phòng</p>
    </div>
    <div style='padding:30px;'>
        <div style='background:#fff8f0;border-left:4px solid #fca311;padding:15px;border-radius:8px;margin-bottom:20px;'>
            <p style='margin:0;color:#856404;font-weight:bold;'>⏳ Đơn đặt phòng đang chờ xác nhận</p>
            <p style='margin:5px 0 0;color:#856404;font-size:14px;'>Vui lòng thanh toán để Admin xử lý đơn đặt phòng của bạn. Sau khi thanh toán, mã mở cửa sẽ được gửi qua email.</p>
        </div>
        <h2 style='color:#2d3436;font-size:18px;margin-bottom:15px;'>📋 Thông tin đặt phòng</h2>
        <table style='width:100%;border-collapse:collapse;'>
            <tr><td style='padding:10px;color:#636e72;width:40%;'>Mã đặt phòng</td><td style='padding:10px;font-weight:bold;color:#ff5a5f;'>{datPhong.MaDatPhong}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Khách hàng</td><td style='padding:10px;font-weight:bold;'>{datPhong.TenKhach}</td></tr>
            <tr><td style='padding:10px;color:#636e72;'>Số điện thoại</td><td style='padding:10px;'>{datPhong.SoDienThoai}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Phòng</td><td style='padding:10px;font-weight:bold;'>{tenPhong}</td></tr>
            <tr><td style='padding:10px;color:#636e72;'>Địa chỉ</td><td style='padding:10px;'>{diaChi}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Ngày nhận phòng</td><td style='padding:10px;'>{datPhong.NgayNhanPhong:dd-MM-yyyy}</td></tr>
            <tr><td style='padding:10px;color:#636e72;'>Thời gian nhận</td><td style='padding:10px;'>{datPhong.ThoiGianNhan?.ToString("HH:mm") ?? "—"}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Thời gian trả</td><td style='padding:10px;'>{datPhong.ThoiGianTra?.ToString("HH:mm") ?? "—"}</td></tr>
        </table>
        <h3 style='color:#2d3436;font-size:16px;margin:20px 0 10px;'>🕐 Khung giờ đã chọn</h3>
        <table style='width:100%;border-collapse:collapse;background:#f8f9fa;border-radius:8px;'>
            {khungGioHtml}
        </table>
        <div style='background:linear-gradient(135deg,#ff5a5f,#e04850);padding:20px;border-radius:12px;text-align:center;margin-top:20px;color:white;'>
            <p style='margin:0;font-size:14px;opacity:0.9;'>Tổng tiền</p>
            <p style='margin:5px 0 0;font-size:28px;font-weight:bold;'>{datPhong.TongTien:N0}đ</p>
        </div>

        <!-- Payment CTA -->
        <div style='background:#e8f5e9;border-radius:12px;padding:20px;margin-top:20px;text-align:center;'>
            <p style='margin:0 0 8px;font-size:15px;color:#2e7d32;font-weight:bold;'>💳 Vui lòng thanh toán để hoàn tất đặt phòng</p>
            <p style='margin:0 0 16px;font-size:13px;color:#666;'>Bấm nút bên dưới để xem thông tin chuyển khoản và tải lên biên lai.</p>
            <a href='{thanhToanUrl}' style='display:inline-block;background:linear-gradient(135deg,#06d6a0,#00b4d8);color:white;padding:14px 36px;border-radius:30px;text-decoration:none;font-weight:bold;font-size:16px;box-shadow:0 4px 15px rgba(6,214,160,0.4);'>
                💸 Thanh toán ngay
            </a>
            <p style='margin:12px 0 0;font-size:12px;color:#999;'>Nội dung chuyển khoản: <strong>{datPhong.MaDatPhong}</strong></p>
        </div>
    </div>
    <div style='background:#f8f9fa;padding:20px;text-align:center;color:#636e72;font-size:13px;'>
        <p style='margin:0;'>Cảm ơn bạn đã sử dụng dịch vụ HomeStay Booking!</p>
        <p style='margin:5px 0 0;'>📞 Hotline: 0909 123 456</p>
    </div>
</div>
</body>
</html>";

            await GuiEmail(datPhong.Email, $"🏠 Xác nhận đặt phòng - {datPhong.MaDatPhong}", body);
        }

        public async Task GuiEmailXacNhan(DatPhong datPhong, string tenPhong, string diaChi, List<string> khungGios)
        {
            if (string.IsNullOrEmpty(datPhong.Email)) return;
            if (string.IsNullOrEmpty(_settings.SenderEmail)) return;

            var khungGioHtml = string.Join("", khungGios.Select(kg => $"<tr><td style='padding:8px 12px;border-bottom:1px solid #f0f0f0;'>{kg}</td></tr>"));

            var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;font-family:Arial,sans-serif;background:#f5f5f5;'>
<div style='max-width:600px;margin:20px auto;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.1);'>
    <div style='background:linear-gradient(135deg,#06d6a0,#00b4d8);padding:30px;text-align:center;color:white;'>
        <h1 style='margin:0;font-size:24px;'>🏠 HomeStay Booking</h1>
        <p style='margin:8px 0 0;opacity:0.9;'>Đặt phòng đã được xác nhận!</p>
    </div>
    <div style='padding:30px;'>
        <div style='background:#d4edda;border-left:4px solid #06d6a0;padding:15px;border-radius:8px;margin-bottom:20px;'>
            <p style='margin:0;color:#155724;font-weight:bold;'>✅ Đặt phòng đã được xác nhận thành công!</p>
        </div>
        <h2 style='color:#2d3436;font-size:18px;margin-bottom:15px;'>📋 Thông tin đặt phòng</h2>
        <table style='width:100%;border-collapse:collapse;'>
            <tr><td style='padding:10px;color:#636e72;width:40%;'>Mã ĐP</td><td style='padding:10px;font-weight:bold;color:#ff5a5f;'>{datPhong.MaDatPhong}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Khách hàng</td><td style='padding:10px;font-weight:bold;'>{datPhong.TenKhach}</td></tr>
            <tr><td style='padding:10px;color:#636e72;'>SĐT</td><td style='padding:10px;'>{datPhong.SoDienThoai}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Email</td><td style='padding:10px;'>{datPhong.Email ?? ""}</td></tr>
            <tr><td style='padding:10px;color:#636e72;'>Phòng</td><td style='padding:10px;font-weight:bold;'>{tenPhong}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Địa chỉ</td><td style='padding:10px;'>{diaChi}</td></tr>
            <tr><td style='padding:10px;color:#636e72;'>Ngày nhận</td><td style='padding:10px;font-weight:bold;'>{datPhong.NgayNhanPhong:dd-MM-yyyy}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Nhận phòng</td><td style='padding:10px;'>{datPhong.ThoiGianNhan?.ToString("HH:mm dd-MM-yyyy") ?? "—"}</td></tr>
            <tr><td style='padding:10px;color:#636e72;'>Trả phòng</td><td style='padding:10px;'>{datPhong.ThoiGianTra?.ToString("HH:mm dd-MM-yyyy") ?? "—"}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Nguồn đặt</td><td style='padding:10px;'>{datPhong.NguonDat}</td></tr>
        </table>
        <h3 style='color:#2d3436;font-size:16px;margin:20px 0 10px;'>🕐 Khung giờ</h3>
        <table style='width:100%;border-collapse:collapse;background:#f8f9fa;border-radius:8px;'>
            {khungGioHtml}
        </table>
        <div style='background:linear-gradient(135deg,#667eea,#764ba2);padding:25px;border-radius:12px;text-align:center;margin:20px 0;color:white;'>
            <p style='margin:0;font-size:14px;opacity:0.9;'>🔐 Mã mở cửa của bạn</p>
            <p style='margin:10px 0 0;font-size:36px;font-weight:bold;letter-spacing:8px;'>{datPhong.MaMoCua ?? "----"}</p>
            <p style='margin:8px 0 0;font-size:12px;opacity:0.8;'>Vui lòng giữ mã này bí mật</p>
        </div>
        <div style='background:linear-gradient(135deg,#ff5a5f,#e04850);padding:20px;border-radius:12px;text-align:center;color:white;'>
            <p style='margin:0;font-size:14px;opacity:0.9;'>Tổng tiền</p>
            <p style='margin:5px 0 0;font-size:28px;font-weight:bold;'>{datPhong.TongTien:N0}đ</p>
        </div>
    </div>
    <div style='background:#f8f9fa;padding:20px;text-align:center;color:#636e72;font-size:13px;'>
        <p style='margin:0;'>Cảm ơn bạn đã sử dụng dịch vụ HomeStay Booking!</p>
        <p style='margin:5px 0 0;'>📞 Hotline: 0909 123 456</p>
    </div>
</div>
</body>
</html>";

            await GuiEmail(datPhong.Email, $"✅ Đặt phòng đã xác nhận - {datPhong.MaDatPhong}", body);
        }

        public async Task GuiEmailThongBaoDatPhong(List<string> danhSachEmail, DatPhong datPhong, string tenPhong, List<string> khungGios)
        {
            if (string.IsNullOrEmpty(_settings.SenderEmail)) return;
            if (!danhSachEmail.Any()) return;

            var khungGioHtml = string.Join("", khungGios.Select(kg => $"<li style='padding:4px 0;'>{kg}</li>"));

            var body = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='margin:0;padding:0;font-family:Arial,sans-serif;background:#f5f5f5;'>
<div style='max-width:600px;margin:20px auto;background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,0.1);'>
    <div style='background:linear-gradient(135deg,#667eea,#764ba2);padding:30px;text-align:center;color:white;'>
        <h1 style='margin:0;font-size:24px;'>📢 Thông báo đặt phòng mới</h1>
    </div>
    <div style='padding:30px;'>
        <p style='font-size:15px;color:#333;'>Có khách mới đặt phòng trên hệ thống:</p>
        <table style='width:100%;border-collapse:collapse;margin:15px 0;'>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Mã đặt phòng</td><td style='padding:10px;font-weight:bold;color:#ff5a5f;'>{datPhong.MaDatPhong}</td></tr>
            <tr><td style='padding:10px;color:#636e72;'>Khách hàng</td><td style='padding:10px;font-weight:bold;'>{datPhong.TenKhach}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>SĐT</td><td style='padding:10px;'>{datPhong.SoDienThoai}</td></tr>
            <tr><td style='padding:10px;color:#636e72;'>Phòng</td><td style='padding:10px;font-weight:bold;'>{tenPhong}</td></tr>
            <tr style='background:#f8f9fa;'><td style='padding:10px;color:#636e72;'>Ngày</td><td style='padding:10px;'>{datPhong.NgayNhanPhong:dd-MM-yyyy}</td></tr>
            <tr><td style='padding:10px;color:#636e72;'>Tổng tiền</td><td style='padding:10px;font-weight:bold;color:#06d6a0;'>{datPhong.TongTien:N0}đ</td></tr>
        </table>
        <p style='font-size:14px;color:#636e72;'>Khung giờ:</p>
        <ul style='padding-left:20px;color:#333;'>{khungGioHtml}</ul>
    </div>
    <div style='background:#f8f9fa;padding:15px;text-align:center;color:#999;font-size:12px;'>
        <p style='margin:0;'>Email tự động từ HomeStay Booking</p>
    </div>
</div>
</body>
</html>";

            foreach (var email in danhSachEmail)
            {
                if (!string.IsNullOrEmpty(email))
                {
                    await GuiEmail(email, $"📢 Đặt phòng mới - {datPhong.MaDatPhong} - {datPhong.TenKhach}", body);
                }
            }
        }

        private async Task GuiEmail(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app
                Console.WriteLine($"[Email Error] {ex.Message}");
            }
        }
    }
}

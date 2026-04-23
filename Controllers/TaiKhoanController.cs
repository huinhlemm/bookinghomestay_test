using Booking_Homestay.Data;
using Booking_Homestay.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking_Homestay.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly UngDungDbContext _db;

        public TaiKhoanController(UngDungDbContext db)
        {
            _db = db;
        }

        public IActionResult DangNhap()
        {
            if (HttpContext.Session.GetString("AdminDangNhap") == "true")
                return RedirectToAction("Index", "QuanTri");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(string tenDangNhap, string matKhau)
        {
            var taiKhoan = await _db.TaiKhoan
                .FirstOrDefaultAsync(t => t.TenDangNhap == tenDangNhap);

            if (taiKhoan == null || !BCrypt.Net.BCrypt.Verify(matKhau, taiKhoan.MatKhauHash))
            {
                ViewBag.Loi = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View();
            }

            if (taiKhoan.BiKhoa)
            {
                ViewBag.Loi = "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin!";
                return View();
            }

            if (!taiKhoan.DaDuyet)
            {
                ViewBag.Loi = "Tài khoản chưa được duyệt. Vui lòng chờ admin xác nhận!";
                return View();
            }

            HttpContext.Session.SetString("AdminDangNhap", "true");
            HttpContext.Session.SetString("TenAdmin", taiKhoan.HoTen);
            HttpContext.Session.SetString("VaiTro", taiKhoan.VaiTro);
            HttpContext.Session.SetInt32("TaiKhoanId", taiKhoan.Id);
            return RedirectToAction("Index", "QuanTri");
        }

        public IActionResult DangKy()
        {
            if (HttpContext.Session.GetString("AdminDangNhap") == "true")
                return RedirectToAction("Index", "QuanTri");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(string tenDangNhap, string matKhau, string xacNhanMatKhau, 
            string hoTen, string? email, string? soDienThoai)
        {
            if (matKhau != xacNhanMatKhau)
            {
                ViewBag.Loi = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            if (matKhau.Length < 6)
            {
                ViewBag.Loi = "Mật khẩu phải có ít nhất 6 ký tự!";
                return View();
            }

            var exists = await _db.TaiKhoan.AnyAsync(t => t.TenDangNhap == tenDangNhap);
            if (exists)
            {
                ViewBag.Loi = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhauHash = BCrypt.Net.BCrypt.HashPassword(matKhau),
                HoTen = hoTen,
                Email = email,
                SoDienThoai = soDienThoai,
                VaiTro = "NhanVien",
                DaDuyet = false,
                BiKhoa = false,
                NgayTao = DateTime.Now
            };

            _db.TaiKhoan.Add(taiKhoan);
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Đăng ký thành công! Vui lòng chờ admin duyệt tài khoản.";
            return RedirectToAction("DangNhap");
        }

        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("DangNhap");
        }
    }
}

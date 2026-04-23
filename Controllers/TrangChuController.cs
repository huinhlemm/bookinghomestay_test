using Booking_Homestay.Data;
using Booking_Homestay.Models;
using Booking_Homestay.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking_Homestay.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly UngDungDbContext _db;

        public TrangChuController(UngDungDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var caiDat = await _db.CaiDatHeThong.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);

            var viewModel = new TrangChuViewModel
            {
                DanhSachKhuVuc = await _db.KhuVuc.ToListAsync(),
                DanhSachPhong = await _db.Phong
                    .Include(p => p.LoaiPhong)
                    .Include(p => p.KhuVuc)
                    .Include(p => p.DanhSachGia!)
                        .ThenInclude(g => g.KhungGio)
                    .Where(p => p.TrangThai)
                    .ToListAsync(),
                DanhSachLoaiPhong = await _db.LoaiPhong.ToListAsync(),
                CaiDat = caiDat!
            };

            return View(viewModel);
        }

        public async Task<IActionResult> TimKiem(string tuKhoa)
        {
            var ketQua = await _db.Phong
                .Include(p => p.LoaiPhong)
                .Include(p => p.KhuVuc)
                .Include(p => p.DanhSachGia!)
                    .ThenInclude(g => g.KhungGio)
                .Where(p => p.TrangThai &&
                    (p.TenPhong.Contains(tuKhoa) || 
                     (p.MoTa != null && p.MoTa.Contains(tuKhoa)) ||
                     (p.DiaChi != null && p.DiaChi.Contains(tuKhoa))))
                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            return View(ketQua);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}

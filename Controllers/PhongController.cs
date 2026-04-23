using Booking_Homestay.Data;
using Booking_Homestay.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Booking_Homestay.Controllers
{
    public class PhongController : Controller
    {
        private readonly UngDungDbContext _db;

        public PhongController(UngDungDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int? maKhuVuc, int? maLoaiPhong, int page = 1)
        {
            var query = _db.Phong
                .Include(p => p.LoaiPhong)
                .Include(p => p.KhuVuc)
                .Include(p => p.DanhSachGia!)
                    .ThenInclude(g => g.KhungGio)
                .Where(p => p.TrangThai);

            if (maKhuVuc.HasValue)
                query = query.Where(p => p.MaKhuVuc == maKhuVuc.Value);
            if (maLoaiPhong.HasValue)
                query = query.Where(p => p.MaLoaiPhong == maLoaiPhong.Value);

            int pageSize = 9;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            ViewBag.DanhSachKhuVuc = await _db.KhuVuc.ToListAsync();
            ViewBag.DanhSachLoaiPhong = await _db.LoaiPhong.ToListAsync();
            ViewBag.MaKhuVuc = maKhuVuc;
            ViewBag.MaLoaiPhong = maLoaiPhong;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            var rooms = await query.OrderBy(p => p.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(rooms);
        }

        public async Task<IActionResult> ChiTiet(int id)
        {
            var phong = await _db.Phong
                .Include(p => p.LoaiPhong)
                .Include(p => p.KhuVuc)
                .Include(p => p.DanhSachGia!)
                    .ThenInclude(g => g.KhungGio)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phong == null) return NotFound();

            var caiDat = await _db.CaiDatHeThong.ToDictionaryAsync(c => c.TenCaiDat, c => c.GiaTri);

            var viewModel = new ChiTietPhongViewModel
            {
                Phong = phong,
                DanhSachGia = phong.DanhSachGia?.ToList() ?? new(),
                DanhSachKhungGio = await _db.KhungGio.OrderBy(k => k.ThuTu).ToListAsync(),
                PhongLienQuan = await _db.Phong
                    .Include(p => p.DanhSachGia!)
                        .ThenInclude(g => g.KhungGio)
                    .Where(p => p.TrangThai && p.Id != id && p.MaKhuVuc == phong.MaKhuVuc)
                    .Take(3)
                    .ToListAsync(),
                CaiDat = caiDat!
            };

            return View(viewModel);
        }

        public async Task<IActionResult> TheoKhuVuc(int id)
        {
            return RedirectToAction("Index", new { maKhuVuc = id });
        }
    }
}

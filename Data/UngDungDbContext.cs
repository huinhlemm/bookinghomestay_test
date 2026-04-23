using Booking_Homestay.Models;
using Microsoft.EntityFrameworkCore;

namespace Booking_Homestay.Data
{
    public class UngDungDbContext : DbContext
    {
        public UngDungDbContext(DbContextOptions<UngDungDbContext> options) : base(options) { }

        public DbSet<Phong> Phong { get; set; }
        public DbSet<LoaiPhong> LoaiPhong { get; set; }
        public DbSet<KhuVuc> KhuVuc { get; set; }
        public DbSet<KhungGio> KhungGio { get; set; }
        public DbSet<GiaPhong> GiaPhong { get; set; }
        public DbSet<DatPhong> DatPhong { get; set; }
        public DbSet<ChiTietDatPhong> ChiTietDatPhong { get; set; }
        public DbSet<ThanhToan> ThanhToan { get; set; }
        public DbSet<KhuyenMai> KhuyenMai { get; set; }
        public DbSet<CaiDatHeThong> CaiDatHeThong { get; set; }
        public DbSet<TaiKhoan> TaiKhoan { get; set; }
        public DbSet<DoanhThu> DoanhThu { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();
        }
    }
}

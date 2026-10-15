using Microsoft.EntityFrameworkCore;
using QuanLySach.DAL.Entities;

namespace QuanLySach.DAL;

public class AppDbContext : DbContext
{
    // Đổi tại đây nếu cần: tên server, tên database, hoặc dùng SQL Login
    // (User Id=sa;Password=...;) thay cho Trusted_Connection.
    private const string ConnectionString =
        @"Server=BAONGUYEN\BAONGUYEN1;Database=QuanLySach;Trusted_Connection=True;TrustServerCertificate=True;";

    public DbSet<Book> Books => Set<Book>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(ConnectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(e =>
        {
            e.Property(b => b.Title).IsRequired();
            e.Property(b => b.Author).IsRequired();
        });
    }
}

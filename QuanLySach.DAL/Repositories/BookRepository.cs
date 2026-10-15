using Microsoft.EntityFrameworkCore;
using QuanLySach.DAL.Entities;

namespace QuanLySach.DAL.Repositories;

public class BookRepository
{
    static BookRepository()
    {
        // Tự tạo database + bảng Books nếu chưa có.
        using var db = new AppDbContext();
        db.Database.EnsureCreated();
    }

    public async Task<List<Book>> LayDanhSachAsync()
    {
        using var db = new AppDbContext();
        return await db.Books.AsNoTracking().OrderBy(b => b.Id).ToListAsync();
    }

    public async Task<Book?> TimTheoIdAsync(int id)
    {
        using var db = new AppDbContext();
        return await db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Book> ThemAsync(Book book)
    {
        using var db = new AppDbContext();
        db.Books.Add(book);
        await db.SaveChangesAsync();
        return book;
    }

    public async Task<bool> SuaAsync(Book book)
    {
        using var db = new AppDbContext();
        var existing = await db.Books.FindAsync(book.Id);
        if (existing == null) return false;

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.Price = book.Price;
        existing.Year = book.Year;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> XoaAsync(int id)
    {
        using var db = new AppDbContext();
        var existing = await db.Books.FindAsync(id);
        if (existing == null) return false;

        db.Books.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }
}

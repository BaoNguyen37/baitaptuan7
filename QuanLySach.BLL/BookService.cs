using QuanLySach.DAL.Entities;
using QuanLySach.DAL.Repositories;

namespace QuanLySach.BLL;

public class BookService
{
    private readonly BookRepository _repo = new();

    // ---------- CRUD ----------
    public Task<List<Book>> LayDanhSachAsync() => _repo.LayDanhSachAsync();

    public Task<Book?> TimTheoIdAsync(int id) => _repo.TimTheoIdAsync(id);

    public async Task<Book> ThemAsync(Book book)
    {
        Validate(book);
        book.Title = book.Title.Trim();
        book.Author = book.Author.Trim();
        return await _repo.ThemAsync(book);
    }

    public async Task SuaAsync(Book book)
    {
        Validate(book);
        book.Title = book.Title.Trim();
        book.Author = book.Author.Trim();
        if (!await _repo.SuaAsync(book))
            throw new BookValidationException("Không tìm thấy sách cần sửa.");
    }

    public async Task XoaAsync(int id)
    {
        if (!await _repo.XoaAsync(id))
            throw new BookValidationException("Không tìm thấy sách cần xóa.");
    }

    // ---------- Validate nghiệp vụ ----------
    public static void Validate(Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title))
            throw new BookValidationException("Tên sách không được để trống.");

        if (string.IsNullOrWhiteSpace(book.Author))
            throw new BookValidationException("Tác giả không được để trống.");

        if (book.Price <= 0)
            throw new BookValidationException("Giá phải lớn hơn 0.");

        if (book.Year < 1000 || book.Year > DateTime.Now.Year)
            throw new BookValidationException($"Năm xuất bản phải từ 1000 đến {DateTime.Now.Year}.");
    }

    // ---------- Thống kê ----------
    public double TinhGiaTrungBinh(List<Book> books)
        => books.Count == 0 ? 0 : books.Average(b => b.Price);

    public double? GiaCaoNhat(List<Book> books)
        => books.Count == 0 ? null : books.Max(b => b.Price);

    public double? GiaThapNhat(List<Book> books)
        => books.Count == 0 ? null : books.Min(b => b.Price);

    public async Task<List<Book>> LocSachTheoNam(int namBatDau, int namKetThuc)
    {
        if (namBatDau > namKetThuc)
            throw new BookValidationException("Năm bắt đầu không được lớn hơn năm kết thúc.");

        var all = await _repo.LayDanhSachAsync();
        return all.Where(b => b.Year >= namBatDau && b.Year <= namKetThuc).ToList();
    }
}

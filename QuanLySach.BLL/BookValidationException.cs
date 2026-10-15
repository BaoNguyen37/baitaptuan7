namespace QuanLySach.BLL;

/// <summary>Lỗi vi phạm quy tắc nghiệp vụ (validate dữ liệu).</summary>
public class BookValidationException : Exception
{
    public BookValidationException(string message) : base(message) { }
}

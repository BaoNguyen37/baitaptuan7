using System.ComponentModel.DataAnnotations;

namespace QuanLySach.DAL.Entities;

public class Book
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Author { get; set; } = string.Empty;

    public double Price { get; set; }

    public int Year { get; set; }
}

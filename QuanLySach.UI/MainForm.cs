using System.Globalization;
using QuanLySach.BLL;                 // chỉ dùng BLL ...
using QuanLySach.DAL.Entities;        // ... và DAL.Entities (không dùng AppDbContext / BookRepository)

namespace QuanLySach.UI;

public class MainForm : Form
{
    private readonly BookService _service = new();

    // trạng thái lọc hiện tại (null = đang xem tất cả)
    private int? _locTu, _locDen;

    private readonly TextBox txtTitle = new();
    private readonly TextBox txtAuthor = new();
    private readonly TextBox txtPrice = new();
    private readonly NumericUpDown nudYear = new();
    private readonly NumericUpDown nudFrom = new();
    private readonly NumericUpDown nudTo = new();
    private readonly Button btnAdd = new();
    private readonly Button btnEdit = new();
    private readonly Button btnDelete = new();
    private readonly Button btnReload = new();
    private readonly Button btnFilter = new();
    private readonly DataGridView dgv = new();
    private readonly Label lblStats = new();

    public MainForm()
    {
        BuildUi();
        Load += async (_, _) => await Run(LoadAllAsync);
    }

    // ================= GIAO DIỆN =================
    private void BuildUi()
    {
        Text = "Quản lý Sách - 3 Project Architecture";
        Font = new Font("Segoe UI", 10F);
        ClientSize = new Size(900, 600);
        MinimumSize = new Size(900, 600);
        StartPosition = FormStartPosition.CenterScreen;

        var lblTitle = new Label
        {
            Text = "QUẢN LÝ SÁCH - KIẾN TRÚC 3 TẦNG (3 PROJECT)",
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Location = new Point(0, 12),
            Size = new Size(900, 40),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        // Hàng 1
        Place(txtTitle, 90, 68, 240);
        Place(txtAuthor, 420, 68, 200);
        Place(txtPrice, 690, 68, 150);

        // Hàng 2
        nudYear.Minimum = 1000; nudYear.Maximum = 9999; nudYear.Value = DateTime.Now.Year;
        Place(nudYear, 90, 108, 80);
        SetupButton(btnAdd, "＋ Thêm", 190, 106, BtnAdd_Click);
        SetupButton(btnEdit, "✎ Sửa", 300, 106, BtnEdit_Click);
        SetupButton(btnDelete, "✖ Xóa", 410, 106, BtnDelete_Click);
        SetupButton(btnReload, "⟳ Tải lại", 520, 106, BtnReload_Click);

        // Hàng 3
        nudFrom.Minimum = 1000; nudFrom.Maximum = 9999; nudFrom.Value = 1900;
        nudTo.Minimum = 1000; nudTo.Maximum = 9999; nudTo.Value = DateTime.Now.Year;
        Place(nudFrom, 90, 152, 80);
        Place(nudTo, 215, 152, 80);
        SetupButton(btnFilter, "🔍 Lọc", 320, 150, BtnFilter_Click);

        // Lưới dữ liệu
        dgv.Location = new Point(20, 195);
        dgv.Size = new Size(860, 340);
        dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.MultiSelect = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.BackgroundColor = SystemColors.ControlLight;
        dgv.RowHeadersVisible = false;
        dgv.SelectionChanged += Dgv_SelectionChanged;

        lblStats.Location = new Point(20, 550);
        lblStats.Size = new Size(860, 30);
        lblStats.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblStats.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblStats.ForeColor = Color.Navy;

        Controls.AddRange(new Control[]
        {
            lblTitle,
            Lbl("Tên sách:", 20, 71), txtTitle,
            Lbl("Tác giả:", 350, 71), txtAuthor,
            Lbl("Giá:", 650, 71), txtPrice,
            Lbl("Năm XB:", 20, 111), nudYear,
            btnAdd, btnEdit, btnDelete, btnReload,
            Lbl("Lọc năm:", 20, 155), nudFrom,
            Lbl("đến", 180, 155), nudTo,
            btnFilter,
            dgv, lblStats
        });
    }

    private static Label Lbl(string text, int x, int y)
        => new() { Text = text, Location = new Point(x, y), AutoSize = true };

    private static void Place(Control c, int x, int y, int w)
    {
        c.Location = new Point(x, y);
        c.Width = w;
    }

    private static void SetupButton(Button b, string text, int x, int y, EventHandler onClick)
    {
        b.Text = text;
        b.Location = new Point(x, y);
        b.Size = new Size(100, 32);
        b.Click += onClick;
    }

    // ================= NẠP DỮ LIỆU =================
    private async Task LoadAllAsync()
    {
        _locTu = _locDen = null;
        Bind(await _service.LayDanhSachAsync());
    }

    private async Task RefreshAsync()
    {
        if (_locTu is int a && _locDen is int b)
            Bind(await _service.LocSachTheoNam(a, b));
        else
            Bind(await _service.LayDanhSachAsync());
    }

    private void Bind(List<Book> list)
    {
        dgv.DataSource = list;
        if (dgv.Columns["Price"] is DataGridViewColumn priceCol)
            priceCol.DefaultCellStyle.Format = "#,##0.##";

        var tb = _service.TinhGiaTrungBinh(list);
        var max = _service.GiaCaoNhat(list);
        var min = _service.GiaThapNhat(list);
        lblStats.Text =
            $"Tổng: {list.Count} cuốn  |  Giá TB: {tb:N0}đ  |  " +
            $"Đắt nhất: {(max.HasValue ? $"{max.Value:N0}đ" : "—")}  |  " +
            $"Rẻ nhất: {(min.HasValue ? $"{min.Value:N0}đ" : "—")}";
    }

    // ================= SỰ KIỆN =================
    private async void BtnAdd_Click(object? s, EventArgs e) => await Run(async () =>
    {
        await _service.ThemAsync(ReadInput());
        ClearInput();
        await RefreshAsync();
    });

    private async void BtnEdit_Click(object? s, EventArgs e) => await Run(async () =>
    {
        var selected = GetSelected();
        if (selected == null)
        {
            MessageBox.Show("Hãy chọn một cuốn sách trong danh sách để sửa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var book = ReadInput();
        book.Id = selected.Id;
        await _service.SuaAsync(book);
        await RefreshAsync();
    });

    private async void BtnDelete_Click(object? s, EventArgs e) => await Run(async () =>
    {
        var selected = GetSelected();
        if (selected == null)
        {
            MessageBox.Show("Hãy chọn một cuốn sách trong danh sách để xóa.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        var ok = MessageBox.Show($"Xóa sách \"{selected.Title}\"?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (ok != DialogResult.Yes) return;

        await _service.XoaAsync(selected.Id);
        ClearInput();
        await RefreshAsync();
    });

    private async void BtnReload_Click(object? s, EventArgs e) => await Run(async () =>
    {
        ClearInput();
        nudFrom.Value = 1900;
        nudTo.Value = DateTime.Now.Year;
        await LoadAllAsync();
    });

    private async void BtnFilter_Click(object? s, EventArgs e) => await Run(async () =>
    {
        int from = (int)nudFrom.Value, to = (int)nudTo.Value;
        var list = await _service.LocSachTheoNam(from, to);   // ném lỗi nếu from > to
        _locTu = from; _locDen = to;
        Bind(list);
    });

    private void Dgv_SelectionChanged(object? s, EventArgs e)
    {
        if (GetSelected() is not Book b) return;
        txtTitle.Text = b.Title;
        txtAuthor.Text = b.Author;
        txtPrice.Text = b.Price.ToString("0.##", CultureInfo.CurrentCulture);
        nudYear.Value = Math.Clamp(b.Year, (int)nudYear.Minimum, (int)nudYear.Maximum);
    }

    // ================= TIỆN ÍCH =================
    private Book? GetSelected() => dgv.CurrentRow?.DataBoundItem as Book;

    private Book ReadInput()
    {
        var text = txtPrice.Text.Trim();
        if (!double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var price) &&
            !double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
            throw new BookValidationException("Giá phải là một số hợp lệ.");

        return new Book
        {
            Title = txtTitle.Text,
            Author = txtAuthor.Text,
            Price = price,
            Year = (int)nudYear.Value
        };
    }

    private void ClearInput()
    {
        txtTitle.Clear();
        txtAuthor.Clear();
        txtPrice.Clear();
        nudYear.Value = DateTime.Now.Year;
        txtTitle.Focus();
    }

    private static async Task Run(Func<Task> action)
    {
        try { await action(); }
        catch (BookValidationException ex)
        {
            MessageBox.Show(ex.Message, "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

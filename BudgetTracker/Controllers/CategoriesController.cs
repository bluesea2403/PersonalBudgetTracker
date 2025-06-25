// Thêm các 'using' cần thiết cho Authorization và Identity
using BudgetTracker.Data;
using BudgetTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

// THÊM DÒNG NÀY: Yêu cầu người dùng phải đăng nhập để truy cập
[Authorize]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    // THÊM DÒNG NÀY: Dùng để lấy thông tin người dùng
    private readonly UserManager<ApplicationUser> _userManager;

    // CHỈNH SỬA HÀM KHỞI TẠO: Thêm UserManager
    public CategoriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Categories - ĐÃ SỬA
    public async Task<IActionResult> Index()
    {
        // Lấy người dùng đang đăng nhập
        var currentUser = await _userManager.GetUserAsync(User);
        // Chỉ lấy danh mục của người dùng này
        var categories = await _context.Categories
                                .Where(c => c.UserId == currentUser.Id || c.UserId == null)
                                .ToListAsync();
        return View(categories);
    }

    // GET: Categories/Details/5 - ĐÃ SỬA
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var currentUser = await _userManager.GetUserAsync(User);
        // Chỉ lấy chi tiết nếu danh mục đó tồn tại VÀ thuộc về người dùng này
        var category = await _context.Categories
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUser.Id);
        if (category == null) return NotFound();
        return View(category);
    }
    // GET: Categories/Create
    public async Task<IActionResult> Create()
    {   
        // Tạo một danh sách các lựa chọn cho dropdown
        // Sử dụng các hằng số từ lớp StaticDetails mà chúng ta đã tạo
        var typeList = new List<SelectListItem>
    {
        new SelectListItem { Text = StaticDetails.Expense, Value = StaticDetails.Expense },
        new SelectListItem { Text = StaticDetails.Income, Value = StaticDetails.Income }
    };

        // Gửi danh sách này sang View qua ViewBag
        ViewBag.TypeList = typeList;
        ViewBag.Icons = GetFontAwesomeIcons();
        ViewBag.CategoryGroups = new SelectList(await _context.CategoryGroups.ToListAsync(), "Name", "Name");
        return View(new Category());
    }

    // POST: Categories/Create - ĐÃ SỬA
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Type,Icon,GroupName")] Category category)
    {
        // Lấy người dùng hiện tại
        var currentUser = await _userManager.GetUserAsync(User);
        // Gán UserId cho đối tượng category ngay từ đầu
        category.UserId = currentUser.Id;

        // Bỏ qua kiểm tra lỗi cho các thuộc tính mà chúng ta không cần bind từ form
        ModelState.Remove("User");
        ModelState.Remove("UserId");

        // Bây giờ, khi kiểm tra IsValid, nó sẽ không báo lỗi thiếu UserId nữa
        if (ModelState.IsValid)
        {
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Nếu có lỗi khác, trả về view với dữ liệu đã nhập
        return View(category);
    }

    private List<string> GetFontAwesomeIcons()
    {
        // Bạn có thể thêm hoặc bớt các icon tùy thích từ trang Font Awesome
        return new List<string>
    {
        "fas fa-utensils", "fas fa-shopping-cart", "fas fa-bus", "fas fa-home",
        "fas fa-plane", "fas fa-file-invoice-dollar", "fas fa-gift", "fas fa-briefcase-medical",
        "fas fa-graduation-cap", "fas fa-paw", "fas fa-film", "fas fa-futbol",
        "fas fa-money-bill-wave", "fas fa-wallet", "fas fa-piggy-bank", "fas fa-chart-line",
        // --- Chi tiêu Thiết yếu ---
        "fas fa-shopping-basket",   // Đi chợ, Tạp hóa
        "fas fa-bolt",              // Hóa đơn (Điện)
        "fas fa-burn",              // Hóa đơn (Gas)
        "fas fa-wifi",              // Hóa đơn (Internet)
        "fas fa-phone-alt",         // Hóa đơn (Điện thoại)
        "fas fa-bus-alt",           // Di chuyển (Công cộng)
        "fas fa-gas-pump",          // Di chuyển (Xăng xe)
        "fas fa-car",               // Di chuyển (Bảo trì xe)

        // --- Cá nhân & Gia đình ---
        "fas fa-tshirt",            // Quần áo
        "fas fa-heartbeat",         // Sức khỏe
        "fas fa-pills",             // Thuốc men
        "fas fa-child",             // Con cái
        "fas fa-user-friends",      // Gia đình, Người thân
        // --- Hưởng thụ & Giải trí ---
        // "fas fa-book-open",         // Sách, Đọc truyện
        "fas fa-gamepad",           // Game
        "fas fa-music",             // Âm nhạc
        "fas fa-plane-departure",   // Du lịch
        "fas fa-cocktail",          // Ăn ngoài, Cafe

        // --- Phát triển & Đầu tư ---
        "fas fa-graduation-cap",    // Giáo dục
        "fas fa-building",          // Bất động sản

        // --- Thu nhập ---
        "fas fa-briefcase",         // Kinh doanh
        "fas fa-star",              // Tiền thưởng
        "fas fa-hand-holding-usd",  // Thu nhập khác
    };
    }
    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == currentUser.Id);

        if (category == null)
        {
            return NotFound(); // Không tìm thấy hoặc không có quyền
        }

        // Chuẩn bị lại dữ liệu cho các dropdown
        var typeList = new List<SelectListItem>
    {
        new SelectListItem { Text = StaticDetails.Expense, Value = StaticDetails.Expense },
        new SelectListItem { Text = StaticDetails.Income, Value = StaticDetails.Income }
    };
        ViewBag.TypeList = typeList;
        ViewBag.Icons = GetFontAwesomeIcons();
        ViewBag.CategoryGroups = new SelectList(await _context.CategoryGroups.ToListAsync(), "Name", "Name");

        return View(category);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Type,Icon,GroupName")] Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);

        // Gán lại UserId để đảm bảo người dùng không thể thay đổi nó
        category.UserId = currentUser.Id;

        ModelState.Remove("User");
        ModelState.Remove("UserId");

        if (ModelState.IsValid)
        {
            try
            {
                // Kiểm tra xem người dùng có quyền cập nhật bản ghi này không
                var categoryInDb = await _context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == id && c.UserId == currentUser.Id);

                if (categoryInDb == null)
                {
                    return NotFound();
                }

                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(category.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // Nếu có lỗi, tải lại dữ liệu cho form và trả về View
        var typeList = new List<SelectListItem>
    {
        new SelectListItem { Text = StaticDetails.Expense, Value = StaticDetails.Expense },
        new SelectListItem { Text = StaticDetails.Income, Value = StaticDetails.Income }
    };
        ViewBag.TypeList = typeList;
        ViewBag.Icons = GetFontAwesomeIcons();

        return View(category);
    }

    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var category = await _context.Categories
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == currentUser.Id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == currentUser.Id);

        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CategoryExists(int id)
    {
        return _context.Categories.Any(e => e.Id == id);
    }

}
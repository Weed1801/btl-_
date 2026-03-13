using Microsoft.AspNetCore.Mvc;
using QuanLyChoThuePhongTro.Services;
using QuanLyChoThuePhongTro.Repositories;
using System.Threading.Tasks;
using System.Linq;

namespace QuanLyChoThuePhongTro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRoomService _roomService;
        private readonly IRentalContractRepository _contractRepository;

        public HomeController(ILogger<HomeController> logger, IRoomService roomService, IRentalContractRepository contractRepository)
        {
            _logger = logger;
            _roomService = roomService;
            _contractRepository = contractRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");
            
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (userRole != "Admin" && userRole != "Landlord")
            {
                return Forbid();
            }

            var rooms = await _roomService.GetAllRoomsAsync();
            var contracts = await _contractRepository.GetAllAsync();

            // Thống kê cơ bản
            ViewBag.TotalRooms = rooms.Count();
            ViewBag.TotalContracts = contracts.Count();
            
            // Giả định trạng thái "Approved" là đang thuê
            var activeContracts = contracts.Where(c => c.Status == "Approved" || c.Status == "Active");
            ViewBag.ActiveContracts = activeContracts.Count();
            
            // Tính doanh thu dự kiến (tổng giá thuê của các hợp đồng đang active)
            ViewBag.TotalRevenue = activeContracts.Sum(c => c.MonthlyPrice);

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}

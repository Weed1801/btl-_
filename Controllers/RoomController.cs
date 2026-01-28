using Microsoft.AspNetCore.Mvc;
using QuanLyChoThuePhongTro.Models;
using QuanLyChoThuePhongTro.Services;
using QuanLyChoThuePhongTro.Repositories;
using System.Threading.Tasks;

namespace QuanLyChoThuePhongTro.Controllers
{
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IRoomRepository _roomRepository;

        public RoomController(IRoomService roomService, IRoomRepository roomRepository)
        {
            _roomService = roomService;
            _roomRepository = roomRepository;
        }

        // GET: Room
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return View(rooms);
        }

        // GET: Room/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Room/Search
        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }

        // POST: Room/Search
        [HttpPost]
        public async Task<IActionResult> Search(string location, decimal? minPrice, decimal? maxPrice)
        {
            var rooms = await _roomService.SearchRoomsAsync(location, minPrice, maxPrice);
            return PartialView("_RoomList", rooms);
        }

        // GET: Room/Create
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            if (!userId.HasValue || userRole != "Landlord")
            {
                return Redirect("/Auth/Login");
            }

            return View();
        }

        // POST: Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Room room)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (!userId.HasValue)
            {
                return Redirect("/Auth/Login");
            }

            room.OwnerId = userId.Value;
            room.CreatedDate = System.DateTime.Now;

            await _roomService.AddRoomAsync(room);
            return RedirectToAction(nameof(Index));
        }

        // GET: Room/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (room.OwnerId != userId)
            {
                return Forbid();
            }

            return View(room);
        }

        // POST: Room/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (room.OwnerId != userId)
            {
                return Forbid();
            }

            room.UpdatedDate = System.DateTime.Now;
            await _roomService.UpdateRoomAsync(room);
            return RedirectToAction(nameof(Index));
        }

        // GET: Room/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (room.OwnerId != userId)
            {
                return Forbid();
            }

            return View(room);
        }

        // POST: Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room != null)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (room.OwnerId != userId)
                {
                    return Forbid();
                }

                await _roomService.DeleteRoomAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

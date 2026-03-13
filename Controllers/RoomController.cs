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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RoomController(IRoomService roomService, IRoomRepository roomRepository, IWebHostEnvironment webHostEnvironment)
        {
            _roomService = roomService;
            _roomRepository = roomRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // Helper: lưu danh sách file ảnh và trả về chuỗi đường dẫn cách nhau bởi dấu phẩy
        private async Task<string?> SaveImagesAsync(IFormFileCollection files)
        {
            if (files == null || files.Count == 0) return null;

            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "rooms");
            Directory.CreateDirectory(uploadFolder);

            var savedPaths = new List<string>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext)) continue;

                    var uniqueName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadFolder, uniqueName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    savedPaths.Add($"/images/rooms/{uniqueName}");
                }
            }
            return savedPaths.Count > 0 ? string.Join(",", savedPaths) : null;
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
        public async Task<IActionResult> Search(RoomFilter filter)
        {
            var rooms = await _roomService.SearchRoomsAsync(filter);
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
        public async Task<IActionResult> Create(Room room, IFormFileCollection roomImages)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            
            if (!userId.HasValue)
            {
                return Redirect("/Auth/Login");
            }

            room.OwnerId = userId.Value;
            room.CreatedDate = System.DateTime.UtcNow;

            // Xử lý upload ảnh
            var savedImages = await SaveImagesAsync(roomImages);
            if (savedImages != null)
            {
                room.ImageUrls = savedImages;
            }

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
        public async Task<IActionResult> Edit(int id, Room room, IFormFileCollection roomImages, string? existingImageUrls)
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

            room.UpdatedDate = System.DateTime.UtcNow;

            // Xử lý upload ảnh mới
            var newImages = await SaveImagesAsync(roomImages);
            if (newImages != null)
            {
                // Ghép ảnh mới vào ảnh cũ
                room.ImageUrls = string.IsNullOrEmpty(existingImageUrls)
                    ? newImages
                    : existingImageUrls + "," + newImages;
            }
            else
            {
                // Giữ nguyên ảnh cũ nếu không upload mới
                room.ImageUrls = existingImageUrls;
            }

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

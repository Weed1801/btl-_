using Microsoft.AspNetCore.Mvc;
using QuanLyChoThuePhongTro.Models;
using QuanLyChoThuePhongTro.Repositories;
using System.Threading.Tasks;

namespace QuanLyChoThuePhongTro.Controllers
{
    public class RentalContractController : Controller
    {
        private readonly IRentalContractRepository _contractRepository;
        private readonly IRoomRepository _roomRepository;

        public RentalContractController(IRentalContractRepository contractRepository, IRoomRepository roomRepository)
        {
            _contractRepository = contractRepository;
            _roomRepository = roomRepository;
        }

        // GET: RentalContract
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            IEnumerable<RentalContract> contracts;

            if (userRole == "Landlord")
            {
                contracts = await _contractRepository.GetByLandlordAsync(userId.Value);
            }
            else if (userRole == "Tenant")
            {
                contracts = await _contractRepository.GetByTenantAsync(userId.Value);
            }
            else
            {
                contracts = await _contractRepository.GetAllAsync();
            }

            // Truyền vai trò xuống View để hiển thị nút đúng theo quyền
            ViewBag.UserRole = userRole;
            ViewBag.UserId = userId.Value;
            return View(contracts);
        }

        // GET: RentalContract/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: RentalContract/Create/5
        public async Task<IActionResult> Create(int roomId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            if (!userId.HasValue || userRole != "Tenant")
            {
                return RedirectToAction("Login", "Auth");
            }

            var room = await _roomRepository.GetByIdAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            var contract = new RentalContract
            {
                RoomId = roomId,
                TenantId = userId.Value,
                LandlordId = room.OwnerId,
                MonthlyPrice = room.Price
            };

            return View(contract);
        }

        // POST: RentalContract/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RentalContract contract)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (contract.TenantId != userId)
            {
                return Forbid();
            }

            contract.Status = "Pending";
            contract.CreatedDate = DateTime.UtcNow;

            // Đảm bảo StartDate và EndDate là UTC (Npgsql yêu cầu)
            contract.StartDate = DateTime.SpecifyKind(contract.StartDate, DateTimeKind.Utc);
            contract.EndDate = DateTime.SpecifyKind(contract.EndDate, DateTimeKind.Utc);

            await _contractRepository.AddAsync(contract);

            return RedirectToAction(nameof(Index));
        }

        // GET: RentalContract/Edit/5  — Chỉ Landlord mới được sửa
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            // Chỉ Landlord sở hữu hợp đồng mới được sửa
            if (userRole != "Landlord" || contract.LandlordId != userId)
            {
                return Forbid();
            }

            return View(contract);
        }

        // POST: RentalContract/Approve/5  — Chỉ Landlord
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            if (!userId.HasValue || userRole != "Landlord")
            {
                return Forbid();
            }

            // Lấy lại từ DB để tránh giả mạo dữ liệu
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            if (contract.LandlordId != userId.Value)
            {
                return Forbid();
            }

            if (contract.Status != "Pending")
            {
                TempData["Error"] = "Chỉ có thể xác nhận hợp đồng đang ở trạng thái chờ xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            contract.Status = "Active";
            contract.UpdatedDate = DateTime.UtcNow;
            await _contractRepository.UpdateAsync(contract);

            // Cập nhật trạng thái phòng sang Đã thuê
            var room = await _roomRepository.GetByIdAsync(contract.RoomId);
            if (room != null)
            {
                room.Status = "Rented";
                room.UpdatedDate = DateTime.UtcNow;
                await _roomRepository.UpdateAsync(room);
            }

            TempData["Success"] = "Đã xác nhận hợp đồng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: RentalContract/Reject/5  — Chỉ Landlord
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            if (!userId.HasValue || userRole != "Landlord")
            {
                return Forbid();
            }

            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            if (contract.LandlordId != userId.Value)
            {
                return Forbid();
            }

            if (contract.Status != "Pending")
            {
                TempData["Error"] = "Chỉ có thể từ chối hợp đồng đang ở trạng thái chờ xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            contract.Status = "Terminated";
            contract.UpdatedDate = DateTime.UtcNow;
            await _contractRepository.UpdateAsync(contract);

            TempData["Success"] = "Đã từ chối hợp đồng.";
            return RedirectToAction(nameof(Index));
        }

        // POST: RentalContract/Edit/5  — Chỉ Landlord
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RentalContract contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            // Lấy lại từ DB tránh giả mạo LandlordId từ form
            var existingContract = await _contractRepository.GetByIdAsync(id);
            if (existingContract == null) return NotFound();

            if (userRole != "Landlord" || existingContract.LandlordId != userId)
            {
                return Forbid();
            }

            // Giữ nguyên các trường không được sửa
            contract.LandlordId = existingContract.LandlordId;
            contract.TenantId = existingContract.TenantId;
            contract.RoomId = existingContract.RoomId;
            contract.CreatedDate = existingContract.CreatedDate;
            contract.UpdatedDate = DateTime.UtcNow;

            // Đảm bảo StartDate và EndDate là UTC (Npgsql yêu cầu)
            contract.StartDate = DateTime.SpecifyKind(contract.StartDate, DateTimeKind.Utc);
            contract.EndDate = DateTime.SpecifyKind(contract.EndDate, DateTimeKind.Utc);

            await _contractRepository.UpdateAsync(contract);

            return RedirectToAction(nameof(Index));
        }

        // GET: RentalContract/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (contract.LandlordId != userId)
            {
                return Forbid();
            }

            return View(contract);
        }

        // POST: RentalContract/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract != null)
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (contract.LandlordId != userId)
                {
                    return Forbid();
                }

                // Nếu xóa hợp đồng đang hiệu lực, trả trạng thái phòng về Available
                var room = await _roomRepository.GetByIdAsync(contract.RoomId);
                if (room != null)
                {
                    room.Status = "Available";
                    await _roomRepository.UpdateAsync(room);
                }

                await _contractRepository.DeleteAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

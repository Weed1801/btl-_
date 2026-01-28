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
            contract.CreatedDate = System.DateTime.Now;

            await _contractRepository.AddAsync(contract);

            return RedirectToAction(nameof(Index));
        }

        // GET: RentalContract/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (contract.LandlordId != userId && contract.TenantId != userId)
            {
                return Forbid();
            }

            return View(contract);
        }

        // POST: RentalContract/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, RentalContract contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (contract.LandlordId != userId && contract.TenantId != userId)
            {
                return Forbid();
            }

            contract.UpdatedDate = System.DateTime.Now;
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

                await _contractRepository.DeleteAsync(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

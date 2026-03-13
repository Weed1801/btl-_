using Microsoft.AspNetCore.Mvc;
using QuanLyChoThuePhongTro.Models;
using QuanLyChoThuePhongTro.Repositories;
using System.Threading.Tasks;

namespace QuanLyChoThuePhongTro.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IRentalContractRepository _contractRepository;

        public PaymentController(IPaymentRepository paymentRepository, IRentalContractRepository contractRepository)
        {
            _paymentRepository = paymentRepository;
            _contractRepository = contractRepository;
        }

        // GET: Payment/ByContract/5
        // Xem lịch sử thanh toán của một hợp đồng
        public async Task<IActionResult> ByContract(int contractId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Kiểm tra người dùng có liên quan đến hợp đồng này không
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null) return NotFound();

            var userRole = HttpContext.Session.GetString("Role");
            bool isLandlord = userRole == "Landlord" && contract.LandlordId == userId;
            bool isTenant = userRole == "Tenant" && contract.TenantId == userId;
            bool isAdmin = userRole == "Admin";

            if (!isLandlord && !isTenant && !isAdmin)
            {
                return Forbid();
            }

            var payments = await _paymentRepository.GetByContractAsync(contractId);
            ViewBag.Contract = contract;
            ViewBag.UserRole = userRole;
            return View(payments);
        }

        // GET: Payment/Create?contractId=5
        // Chỉ Tenant mới được tạo thanh toán
        public async Task<IActionResult> Create(int contractId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            if (!userId.HasValue || userRole != "Tenant")
            {
                return RedirectToAction("Login", "Auth");
            }

            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null) return NotFound();

            // Chỉ người thuê của hợp đồng này mới được thanh toán
            if (contract.TenantId != userId.Value)
            {
                return Forbid();
            }

            // Chỉ cho thanh toán hợp đồng đang Active
            if (contract.Status != "Active")
            {
                TempData["Error"] = "Chỉ có thể thanh toán cho hợp đồng đang hoạt động.";
                return RedirectToAction("ByContract", new { contractId });
            }

            var payment = new Payment
            {
                ContractId = contractId,
                Amount = contract.MonthlyPrice,
                PaymentDate = DateTime.UtcNow
            };

            ViewBag.Contract = contract;
            return View(payment);
        }

        // POST: Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            if (!userId.HasValue || userRole != "Tenant")
            {
                return Forbid();
            }

            // Lấy lại từ DB để tránh giả mạo
            var contract = await _contractRepository.GetByIdAsync(payment.ContractId);
            if (contract == null || contract.TenantId != userId.Value)
            {
                return Forbid();
            }

            payment.PaymentDate = DateTime.UtcNow;
            payment.Status = "Completed";
            payment.CreatedDate = DateTime.UtcNow;

            await _paymentRepository.AddAsync(payment);

            // Cập nhật LastPaymentDate trên hợp đồng
            contract.LastPaymentDate = payment.PaymentDate;
            await _contractRepository.UpdateAsync(contract);

            TempData["Success"] = $"Thanh toán {payment.Amount:N0} VNĐ thành công!";
            return RedirectToAction("ByContract", new { contractId = payment.ContractId });
        }

        // POST: Payment/QuickPay/5
        // Thanh toán nhanh 1 click (Thành công mặc định)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickPay(int contractId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var userRole = HttpContext.Session.GetString("Role");

            if (!userId.HasValue || userRole != "Tenant")
            {
                return Forbid();
            }

            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null || contract.TenantId != userId.Value)
            {
                return Forbid();
            }

            if (contract.Status != "Active")
            {
                TempData["Error"] = "Chỉ có thể thanh toán cho hợp đồng đang hoạt động.";
                return RedirectToAction("Index", "RentalContract");
            }

            var payment = new Payment
            {
                ContractId = contractId,
                Amount = contract.MonthlyPrice,
                PaymentDate = DateTime.UtcNow,
                PaymentMethod = "Quick Pay",
                Status = "Completed", // Mặc định thành công
                Notes = "Thanh toán nhanh qua hệ thống",
                CreatedDate = DateTime.UtcNow
            };

            await _paymentRepository.AddAsync(payment);

            // Cập nhật ngày thanh toán gần nhất
            contract.LastPaymentDate = payment.PaymentDate;
            await _contractRepository.UpdateAsync(contract);

            TempData["Success"] = $"Đã thanh toán nhanh {payment.Amount:N0} VNĐ thành công!";
            return RedirectToAction("ByContract", new { contractId });
        }

        // GET: Payment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Auth");

            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null) return NotFound();

            var contract = payment.Contract;
            var userRole = HttpContext.Session.GetString("Role");
            bool hasAccess = userRole == "Admin"
                || (contract != null && (contract.LandlordId == userId || contract.TenantId == userId));

            if (!hasAccess) return Forbid();

            return View(payment);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using WelfareLink.Models;
using WelfareLink.Services;
using WelfareLink.ViewModels;

namespace WelfareLink.Controllers
{
    public class WelfareProgramController : Controller
    {
        private readonly WelfareApiClient _api;

        public WelfareProgramController(WelfareApiClient api)
        {
            _api = api;
        }

        // GET: Program
        public async Task<IActionResult> Index()
        {
            var programs = await _api.GetAllProgramsAsync();
            return View(programs);
        }

        // GET: Program/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var programs = await _api.GetAllProgramsAsync();
            var allResources = await _api.GetAllResourcesAsync();
            ViewBag.AllResources = allResources;
            return View(programs);
        }

        // GET: Program/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "[400 Bad Request] Programme ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = await _api.GetProgramByIdAsync(id.Value);
            if (viewModel == null)
            {
                TempData["ErrorMessage"] = "[404 Not Found] Programme not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: Program/Manage
        public IActionResult Manage()
        {
            var newProgram = new WelfareProgram
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddMonths(6)
            };
            return View(newProgram);
        }

        // POST: Program/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage([Bind("ProgramID,Title,Description,StartDate,EndDate,Budget,MaxBenefitPerCitizen,Status,EligibleGender,RequiredDocuments")] WelfareProgram program)
        {
            if (program.ProgramID == 0) ModelState.Remove("Status");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "[400 Bad Request] Please fill all required fields correctly.";
                return View(program);
            }

            string? error;
            if (program.ProgramID == 0)
            {
                error = await _api.AddProgramAsync(program);
                if (error == null) { TempData["SuccessMessage"] = "Programme created successfully!"; return RedirectToAction(nameof(Index)); }
            }
            else
            {
                error = await _api.UpdateProgramAsync(program);
                if (error == null) { TempData["SuccessMessage"] = "Programme updated successfully!"; return RedirectToAction(nameof(Index)); }
            }

            TempData["ErrorMessage"] = $"[400 Bad Request] {error}";
            return View(program);
        }

        // GET: Program/Create
        public IActionResult Create()
        {
            return View(new WelfareProgram { StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(6) });
        }

        // POST: Program/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProgramID,Title,Description,StartDate,EndDate,Budget,MaxBenefitPerCitizen")] WelfareProgram program)
        {
            ModelState.Remove("Status");
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "[400 Bad Request] Please fill all required fields correctly.";
                return View(program);
            }

            var error = await _api.AddProgramAsync(program);
            if (error == null)
            {
                TempData["SuccessMessage"] = "Programme created successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = $"[400 Bad Request] {error}";
            return View(program);
        }

        // GET: Program/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "[400 Bad Request] Programme ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var detail = await _api.GetProgramByIdAsync(id.Value);
            if (detail?.Program == null)
            {
                TempData["ErrorMessage"] = "[404 Not Found] Programme not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(detail.Program);
        }

        // POST: Program/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProgramID,Title,Description,StartDate,EndDate,Budget,MaxBenefitPerCitizen,Status,EligibleGender,RequiredDocuments")] WelfareProgram program)
        {
            if (id != program.ProgramID)
            {
                TempData["ErrorMessage"] = "[400 Bad Request] Invalid programme ID.";
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "[400 Bad Request] Please fill all required fields correctly.";
                return View(program);
            }

            var error = await _api.UpdateProgramAsync(program);
            if (error == null)
            {
                TempData["SuccessMessage"] = "Programme updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = $"[400 Bad Request] {error}";
            return View(program);
        }

        // POST: Program/Suspend
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Suspend(int id)
        {
            var error = await _api.SuspendProgramAsync(id);
            if (error == null)
            {
                TempData["SuccessMessage"] = "Programme suspended successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = $"[400 Bad Request] {error}";
            return RedirectToAction(nameof(Index));
        }

        // GET: Program/BudgetMonitoring
        public async Task<IActionResult> BudgetMonitoring()
        {
            var dashboard = await _api.GetBudgetMonitoringAsync();
            return View(dashboard ?? new BudgetDashboardViewModel { ProgramBudgets = [] });
        }

        // GET: Program/Performance
        public async Task<IActionResult> Performance()
        {
            var performance = await _api.GetProgramPerformanceAsync();
            return View(performance);
        }
    }
}

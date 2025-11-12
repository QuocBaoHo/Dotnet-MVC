using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HRStaffManagement.Models;
using Microsoft.AspNetCore.Hosting;

namespace HRStaffManagement.Controllers
{
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public StaffController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Staff
        public async Task<IActionResult> Index()
        {
            return View(await _context.Staff.ToListAsync());
        }

        // GET: Staff/Details/S001
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff
                .FirstOrDefaultAsync(m => m.StaffId == id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // GET: Staff/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,StaffId,StaffName,Email,PhoneNumber,StartingDate,Photo")] Staff staff)
        {
            if (ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                // Handle file upload
                if (staff.Photo != null && staff.Photo.Length > 0)
                {
                    string uniqueFileName = await UploadFile(staff.Photo);
                    staff.PhotoPath = uniqueFileName;
                }

                _context.Add(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        // GET: Staff/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.StaffId == id);
            if (staff == null) return NotFound();

            var model = new EditStaffViewModel
            {
                Id = staff.Id,
                StaffId = staff.StaffId,
                StaffName = staff.StaffName,
                Email = staff.Email,
                PhoneNumber = staff.PhoneNumber,
                StartingDate = staff.StartingDate,
                PhotoPath = staff.PhotoPath
            };

            ModelState.Clear();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditStaffViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var staff = await _context.Staff.FindAsync(model.Id);
            if (staff == null) return NotFound();

            staff.StaffId = model.StaffId;
            staff.StaffName = model.StaffName;
            staff.Email = model.Email;
            staff.PhoneNumber = model.PhoneNumber;
            staff.StartingDate = model.StartingDate;

            if (model.Photo != null && model.Photo.Length > 0)
            {
                if (!string.IsNullOrEmpty(staff.PhotoPath))
                    DeleteFile(staff.PhotoPath);

                staff.PhotoPath = await UploadFile(model.Photo);
            }

            _context.Update(staff);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




        // GET: Staff/Delete/5
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff
                .FirstOrDefaultAsync(m => m.StaffId == id);
            if (staff == null)
            {
                return NotFound();
            }

            ModelState.Clear();
            return View(staff);
        }

        // POST: Staff/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var staff = await _context.Staff.FindAsync(id);
            if (staff != null)
            {
                // Delete associated photo if exists
                if (!string.IsNullOrEmpty(staff.PhotoPath))
                {
                    DeleteFile(staff.PhotoPath);
                }

                _context.Staff.Remove(staff);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StaffExists(int id)
        {
            return _context.Staff.Any(e => e.Id == id);
        }

        private async Task<string> UploadFile(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "staff");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Return relative URL instead of just the file name
            return Path.Combine("uploads", "staff", uniqueFileName).Replace("\\", "/");
        }

        private void DeleteFile(string filePath)
        {
            string absolutePath = Path.Combine(_webHostEnvironment.WebRootPath, filePath.TrimStart('/'));
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }
    }
}
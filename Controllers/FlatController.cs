using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartSociety.Models;
using SmartSociety.Models.Enums;

namespace SmartSociety.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FlatController : Controller
    {
        private readonly AppDbContext _context;

        public FlatController(AppDbContext context)
        {
            _context = context;
        }


        // GET: Flat
        public async Task<IActionResult> Index()
        {
            var flats = await _context.Flats
                .OrderBy(f => f.BlockName)
                .ThenBy(f => f.FlatNumber)
                .ToListAsync();

            return View(flats);
        }


        // GET: Flat/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: Flat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Flat flat)
        {
            if (!ModelState.IsValid)
            {
                return View(flat);
            }


            // Check duplicate flat
            bool flatExists = await _context.Flats.AnyAsync(f =>
                f.BlockName == flat.BlockName &&
                f.FlatNumber == flat.FlatNumber);

            if (flatExists)
            {
                ModelState.AddModelError(
                    "",
                    "This flat already exists in this block."
                );

                return View(flat);
            }


            // New flat is initially unoccupied
            flat.IsOccupied = false;

            _context.Flats.Add(flat);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Flat created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // GET: Flat/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flat = await _context.Flats.FindAsync(id);

            if (flat == null)
            {
                return NotFound();
            }

            return View(flat);
        }


        // POST: Flat/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Flat flat)
        {
            if (id != flat.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(flat);
            }


            bool duplicateExists = await _context.Flats.AnyAsync(f =>
                f.Id != flat.Id &&
                f.BlockName == flat.BlockName &&
                f.FlatNumber == flat.FlatNumber);

            if (duplicateExists)
            {
                ModelState.AddModelError(
                    "",
                    "This flat already exists in this block."
                );

                return View(flat);
            }


            try
            {
                _context.Flats.Update(flat);

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Flats.AnyAsync(f => f.Id == id))
                {
                    return NotFound();
                }

                throw;
            }

            TempData["Success"] = "Flat updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // GET: Flat/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flat = await _context.Flats
                .FirstOrDefaultAsync(f => f.Id == id);

            if (flat == null)
            {
                return NotFound();
            }

            return View(flat);
        }


        // POST: Flat/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flat = await _context.Flats.FindAsync(id);

            if (flat == null)
            {
                return NotFound();
            }


            // Don't delete occupied flat
            if (flat.IsOccupied)
            {
                TempData["Error"] =
                    "Occupied flat cannot be deleted.";

                return RedirectToAction(nameof(Index));
            }


            _context.Flats.Remove(flat);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Flat deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
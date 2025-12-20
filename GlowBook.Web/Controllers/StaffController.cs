using GlowBook.Model.Data;
using GlowBook.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowBook.Web.Controllers;

[Authorize(Roles = "Admin,Owner")]
public class StaffController : Controller
{
    private readonly AppDbContext _ctx;

    public StaffController(AppDbContext ctx) => _ctx = ctx;

    public async Task<IActionResult> Index(string? q = null)
    {
        var query = _ctx.Staff.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s =>
                s.Name.Contains(q) ||
                (s.Email != null && s.Email.Contains(q)));

        var list = await query.OrderBy(s => s.Name).ToListAsync();
        ViewBag.Q = q;
        return View(list);
    }

    public IActionResult Create() => View(new Staff());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Staff model)
    {
        if (!ModelState.IsValid) return View(model);

        _ctx.Staff.Add(model);
        await _ctx.SaveChangesAsync();

        TempData["Message"] = "Medewerker aangemaakt.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var staff = await _ctx.Staff.FirstOrDefaultAsync(s => s.Id == id);
        if (staff == null) return NotFound();

        return View(staff);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Staff model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        _ctx.Entry(model).State = EntityState.Modified;
        await _ctx.SaveChangesAsync();

        TempData["Message"] = "Medewerker aangepast.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var staff = await _ctx.Staff.FirstOrDefaultAsync(s => s.Id == id);
        if (staff == null) return NotFound();

        return View(staff);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var staff = await _ctx.Staff.FirstOrDefaultAsync(s => s.Id == id);
        if (staff == null) return NotFound();

        _ctx.Staff.Remove(staff);
        await _ctx.SaveChangesAsync();

        TempData["Message"] = "Medewerker verwijderd.";
        return RedirectToAction(nameof(Index));
    }
}

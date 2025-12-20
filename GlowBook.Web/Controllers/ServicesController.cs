using GlowBook.Model.Data;
using GlowBook.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlowBook.Web.Controllers;

[Authorize]
public class ServicesController : Controller
{
    private readonly AppDbContext _ctx;

    private static readonly string[] Categories = new[]
    {
        "Gezicht",
        "Nagels",
        "Pedicure",
        "Massage",
        "Lash & Brow",
        "Waxen"
    };

    public ServicesController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IActionResult> Index(string? q = null, string sort = "name")
    {
        var query = _ctx.Services.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s => s.Name.Contains(q));

        query = sort switch
        {
            "name_desc" => query.OrderByDescending(s => s.Name),
            _ => query.OrderBy(s => s.Name)
        };

        ViewBag.Q = q;
        ViewBag.Sort = sort;

        return View(await query.ToListAsync());
    }


    public async Task<IActionResult> Details(int id)
    {
        var service = await _ctx.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null) return NotFound();
        return View(service);
    }

    [Authorize(Policy = "RequireAdmin")]
    public IActionResult Create()
    {
        FillCategories();          // dropdown vullen
        return View(new Service());
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Service model)
    {
        if (!ModelState.IsValid)
        {
            FillCategories(model.Category);   // weer vullen bij fout
            return View(model);
        }

        _ctx.Services.Add(model);
        await _ctx.SaveChangesAsync();
        TempData["Message"] = "Dienst aangemaakt.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Edit(int id)
    {
        var service = await _ctx.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null) return NotFound();

        FillCategories(service.Category);     // huidige categorie selecteren
        return View(service);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Service model)
    {
        if (!ModelState.IsValid)
        {
            FillCategories(model.Category);
            return View(model);
        }

        var service = await _ctx.Services.FirstOrDefaultAsync(s => s.Id == model.Id);
        if (service == null) return NotFound();

        service.Name = model.Name;
        service.DurationMinutes = model.DurationMinutes;
        service.Price = model.Price;
        service.Category = model.Category;
        service.Description = model.Description;

        await _ctx.SaveChangesAsync();
        TempData["Message"] = "Dienst gewijzigd.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _ctx.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null) return NotFound();
        return View(service);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Policy = "RequireAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var service = await _ctx.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null) return NotFound();

        _ctx.Services.Remove(service);
        await _ctx.SaveChangesAsync();

        TempData["Message"] = "Dienst verwijderd.";
        return RedirectToAction(nameof(Index));
    }


    // helper om ViewBag.Categories te vullen
    private void FillCategories(string? selected = null)
    {
        ViewBag.Categories = new SelectList(Categories, selected);
    }
}

using GlowBook.Model.Data;
using GlowBook.Model.Entities;
using GlowBook.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowBook.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly AppDbContext _ctx;

    public CustomersController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<IActionResult> Index(string? q = null, string sort = "name")
    {
        var query = _ctx.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Name.Contains(q) ||
                                     (c.Email != null && c.Email.Contains(q)) ||
                                     (c.Phone != null && c.Phone.Contains(q)));

        query = sort switch
        {
            "name_desc" => query.OrderByDescending(c => c.Name),
            _ => query.OrderBy(c => c.Name)
        };

        ViewBag.Q = q;
        ViewBag.Sort = sort;

        return View(await query.ToListAsync());
    }


    public async Task<IActionResult> Details(int id)
    {
        var customer = await _ctx.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [Authorize(Policy = "RequireAdmin")]
    public IActionResult Create() => View(new CustomerEditViewModel());

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var customer = new Customer
        {
            Name = model.Name,
            Phone = model.Phone,
            Email = model.Email,
            Notes = model.Notes
        };

        _ctx.Customers.Add(customer);
        await _ctx.SaveChangesAsync();
        TempData["Message"] = "Klant aangemaakt.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _ctx.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return NotFound();

        var model = new CustomerEditViewModel
        {
            Id = customer.Id,
            Name = customer.Name,
            Phone = customer.Phone,
            Email = customer.Email,
            Notes = customer.Notes
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var customer = await _ctx.Customers.FirstOrDefaultAsync(c => c.Id == model.Id);
        if (customer == null) return NotFound();

        customer.Name = model.Name;
        customer.Phone = model.Phone;
        customer.Email = model.Email;
        customer.Notes = model.Notes;

        await _ctx.SaveChangesAsync();
        TempData["Message"] = "Klant gewijzigd.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _ctx.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return NotFound();
        return View(customer);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Policy = "RequireAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await _ctx.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return NotFound();

        _ctx.Customers.Remove(customer);
        await _ctx.SaveChangesAsync();

        TempData["Message"] = "Klant verwijderd.";
        return RedirectToAction(nameof(Index));
    }
}

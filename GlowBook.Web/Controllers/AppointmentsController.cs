using GlowBook.Model.Data;
using GlowBook.Model.Entities;
using GlowBook.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowBook.Web.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly AppDbContext _ctx;

    public AppointmentsController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    
    private DateTime RoundToMinute(DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
    }

    public async Task<IActionResult> Index(AppointmentFilterViewModel filter)
    {
        var query = _ctx.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .AsQueryable();

        if (filter.From.HasValue)
            query = query.Where(a => a.Start >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(a => a.Start <= filter.To.Value);

        if (filter.StaffId.HasValue)
            query = query.Where(a => a.StaffId == filter.StaffId.Value);

        switch (filter.SortOrder)
        {
            case "date_desc":
                query = query.OrderByDescending(a => a.Start);
                break;
            case "customer":
                query = query.OrderBy(a => a.Customer.Name);
                break;
            default:
                query = query.OrderBy(a => a.Start);
                break;
        }

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var model = new PagedResult<Appointment>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = total
        };

        ViewBag.Filter = filter;
        ViewBag.StaffList = await _ctx.Staff.OrderBy(s => s.Name).ToListAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_AppointmentListPartial", model);

        return View(model);
    }

    [Authorize(Policy = "CanManageAppointments")]
    public async Task<IActionResult> Create()
    {
        await FillDropdownsAsync();

        var now = RoundToMinute(DateTime.Now);

        var model = new Appointment
        {
            Start = now,
            End = now.AddMinutes(60),
            Status = "Ingepland"
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageAppointments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment model)
    {
        if (!ModelState.IsValid)
        {
            await FillDropdownsAsync();
            return View(model);
        }

        _ctx.Appointments.Add(model);
        await _ctx.SaveChangesAsync();
        TempData["Message"] = "Afspraak aangemaakt.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "CanManageAppointments")]
    public async Task<IActionResult> Edit(int id)
    {
        var appt = await _ctx.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null) return NotFound();

        await FillDropdownsAsync();
        return View(appt);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageAppointments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Appointment model)
    {
        if (!ModelState.IsValid)
        {
            await FillDropdownsAsync();
            return View(model);
        }

        var appt = await _ctx.Appointments.FirstOrDefaultAsync(a => a.Id == model.Id);
        if (appt == null) return NotFound();

        appt.CustomerId = model.CustomerId;
        appt.StaffId = model.StaffId;
        appt.Start = model.Start;
        appt.End = model.End;
        appt.Status = model.Status;

        await _ctx.SaveChangesAsync();
        TempData["Message"] = "Afspraak gewijzigd.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "CanManageAppointments")]
    public async Task<IActionResult> Delete(int id)
    {
        var appt = await _ctx.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null) return NotFound();
        return View(appt);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageAppointments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Appointment model)
    {
        var appt = await _ctx.Appointments.FirstOrDefaultAsync(a => a.Id == model.Id);
        if (appt == null) return NotFound();

        _ctx.Appointments.Remove(appt);
        await _ctx.SaveChangesAsync();
        TempData["Message"] = "Afspraak verwijderd.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var appt = await _ctx.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appt == null) return NotFound();
        return View(appt);
    }

    private async Task FillDropdownsAsync()
    {
        ViewBag.Customers = await _ctx.Customers.OrderBy(c => c.Name).ToListAsync();
        ViewBag.Staff = await _ctx.Staff.OrderBy(s => s.Name).ToListAsync();
    }
}

using GlowBook.Model.Data;
using GlowBook.Model.Entities;
using GlowBook.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GlowBook.Model.Helpers;

namespace GlowBook.Web.Controllers;

[Authorize(Policy = "CanManageAppointments")]
public class AppointmentsController : Controller
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    // INDEX MET FILTERS
    public async Task<IActionResult> Index(AppointmentFilterViewModel filter, int page = 1)
    {
        const int pageSize = 10;

        if (!filter.From.HasValue)
            filter.From = DateTime.Today;

        if (!filter.To.HasValue)
            filter.To = DateTime.Today.AddDays(7);

        var query = _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.AppointmentServices)
                .ThenInclude(x => x.Service)
            .AsQueryable();

        if (filter.From.HasValue)
        {
            var fromDate = filter.From.Value.Date;
            query = query.Where(a => a.Start >= fromDate);
        }

        if (filter.To.HasValue)
        {
            var toDateExclusive = filter.To.Value.Date.AddDays(1);
            query = query.Where(a => a.Start < toDateExclusive);
        }

        if (filter.CustomerId.HasValue)
            query = query.Where(a => a.CustomerId == filter.CustomerId.Value);

        if (filter.StaffId.HasValue)
            query = query.Where(a => a.StaffId == filter.StaffId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(a => a.Status == filter.Status);

        int totalItems = await query.CountAsync();

        var appointments = await query
            .OrderBy(a => a.Start)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PagedResult<Appointment>
        {
            Items = appointments,
            PageNumber = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };

        await LoadFilterListsAsync(filter);
        ViewBag.Filter = filter;

        return View(model);
    }

    public async Task<IActionResult> ListPartial(AppointmentFilterViewModel filter)
    {
        var query = _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.AppointmentServices)
                .ThenInclude(x => x.Service)
            .AsQueryable();

        if (filter.From.HasValue)
        {
            var fromDate = filter.From.Value.Date;
            query = query.Where(a => a.Start >= fromDate);
        }

        if (filter.To.HasValue)
        {
            var toDateExclusive = filter.To.Value.Date.AddDays(1);
            query = query.Where(a => a.Start < toDateExclusive);
        }

        if (filter.CustomerId.HasValue)
            query = query.Where(a => a.CustomerId == filter.CustomerId.Value);

        if (filter.StaffId.HasValue)
            query = query.Where(a => a.StaffId == filter.StaffId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(a => a.Status == filter.Status);

        var results = await query
            .OrderBy(a => a.Start)
            .ToListAsync();

        return PartialView("_AppointmentsList", results);
    }



    // GET: Create
    public async Task<IActionResult> Create()
    {
        var vm = new AppointmentEditViewModel
        {
            Date = DateTime.Today,
            StartTime = new TimeSpan(9, 0, 0),
            DurationMinutes = 60,
            Status = "Ingepland"
        };

        await LoadDropDowns(vm);

        return View(vm);
    }

    // POST: Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await LoadDropDowns(vm);
            return View(vm);
        }

        var start = vm.Date.Date + vm.StartTime;
        var end = start.AddMinutes(vm.DurationMinutes);

        var appointment = new Appointment
        {
            CustomerId = vm.CustomerId,
            StaffId = vm.StaffId,
            Start = start,
            End = end,
            Status = string.IsNullOrWhiteSpace(vm.Status) ? "Ingepland" : vm.Status
        };

        appointment.AppointmentServices.Add(new AppointmentService
        {
            ServiceId = vm.ServiceId,
            Appointment = appointment
        });

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // Dropdowns voor Create/Edit
    private async Task LoadDropDowns(AppointmentEditViewModel vm)
    {
        vm.Customers = new SelectList(
            await _context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync(),
            "Id", "Name", vm.CustomerId);

        vm.Staff = new SelectList(
            await _context.Staff
                .OrderBy(s => s.Name)
                .ToListAsync(),
            "Id", "Name", vm.StaffId);

        vm.Services = new SelectList(
            await _context.Services
                .OrderBy(s => s.Name)
                .ToListAsync(),
            "Id", "Name", vm.ServiceId);
    }

    // Dropdowns voor filterbalk
    private async Task LoadFilterListsAsync(AppointmentFilterViewModel filter)
    {
        filter.Customers = new SelectList(
            await _context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync(),
            "Id", "Name", filter.CustomerId);

        filter.Staff = new SelectList(
            await _context.Staff
                .OrderBy(s => s.Name)
                .ToListAsync(),
            "Id", "Name", filter.StaffId);

        var statusOptions = new List<string>
        {
            "Ingepland",
            "Afgewerkt",
            "Geannuleerd"
        };

        filter.Statuses = new SelectList(statusOptions, filter.Status);
    }
}

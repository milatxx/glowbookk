using GlowBook.Model.Data;
using GlowBook.Model.Entities;
using GlowBook.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GlowBook.Web.Controllers;

[Authorize(Policy = "CanManageAppointments")]
public class AppointmentsController : Controller
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    // LIST
    public async Task<IActionResult> Index()
    {
        var items = await _context.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.AppointmentServices)
                .ThenInclude(x => x.Service)
            .OrderBy(a => a.Start)
            .ToListAsync();

        return View(items);
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
}

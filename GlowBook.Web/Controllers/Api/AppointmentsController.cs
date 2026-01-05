using GlowBook.Model.Data;
using GlowBook.Model.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlowBook.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AppointmentsController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public AppointmentsController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    // GET: api/appointments
    [HttpGet]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<ActionResult<IEnumerable<Appointment>>> GetAll(DateTime? from = null, DateTime? to = null)
    {
        var query = _ctx.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.AppointmentServices)
                .ThenInclude(x => x.Service)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(a => a.Start >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Start <= to.Value);

        var list = await query.ToListAsync();
        return Ok(list);
    }

    // GET: api/appointments/5
    [HttpGet("{id}")]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<ActionResult<Appointment>> Get(int id)
    {
        var appt = await _ctx.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Staff)
            .Include(a => a.AppointmentServices)
                 .ThenInclude(x => x.Service)
            .FirstOrDefaultAsync(a => a.Id == id);


        if (appt == null)
            return NotFound();

        return appt;
    }

    // POST: api/appointments
    [HttpPost]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<ActionResult<Appointment>> Post(Appointment appt)
    {
        _ctx.Appointments.Add(appt);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = appt.Id }, appt);
    }

    // PUT: api/appointments/5
    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<IActionResult> Put(int id, Appointment appt)
    {
        if (id != appt.Id)
            return BadRequest();

        _ctx.Entry(appt).State = EntityState.Modified;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/appointments/5
    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageAppointments")]
    public async Task<IActionResult> DeleteApi(int id)
    {
        var appt = await _ctx.Appointments.FirstOrDefaultAsync(a => a.Id == id);
        if (appt == null)
            return NotFound();

        _ctx.Appointments.Remove(appt);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}

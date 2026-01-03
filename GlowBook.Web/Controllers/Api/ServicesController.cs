using GlowBook.Model.Data;
using GlowBook.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace GlowBook.Web.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public ServicesController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    // GET: api/services
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Service>>> GetAll()
    {
        var services = await _ctx.Services
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return Ok(services);
    }

    // GET: api/services/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Service>> Get(int id)
    {
        var service = await _ctx.Services
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
            return NotFound();

        return service;
    }

    // POST: api/services
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<Service>> Post(Service service)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _ctx.Services.Add(service);
        await _ctx.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = service.Id }, service);
    }

    // PUT: api/services/5
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Put(int id, Service service)
    {
        if (id != service.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _ctx.Entry(service).State = EntityState.Modified;

        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _ctx.Services.AnyAsync(s => s.Id == id))
                return NotFound();

            throw;
        }

        return NoContent();
    }

    // DELETE: api/services/5
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _ctx.Services.FirstOrDefaultAsync(s => s.Id == id);
        if (service == null)
            return NotFound();

        _ctx.Services.Remove(service);
        await _ctx.SaveChangesAsync();

        return NoContent();
    }
}

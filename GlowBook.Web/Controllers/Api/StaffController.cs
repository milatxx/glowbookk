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
public class StaffController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public StaffController(AppDbContext ctx) => _ctx = ctx;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Staff>>> GetAll()
        => await _ctx.Staff.OrderBy(s => s.Name).ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Staff>> GetById(int id)
    {
        var staff = await _ctx.Staff.FindAsync(id);
        if (staff == null) return NotFound();
        return Ok(staff);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<ActionResult<Staff>> Create([FromBody] Staff staff)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _ctx.Staff.Add(staff);
        await _ctx.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = staff.Id }, staff);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<IActionResult> Update(int id, [FromBody] Staff staff)
    {
        if (id != staff.Id) return BadRequest("Id mismatch.");
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _ctx.Entry(staff).State = EntityState.Modified;

        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _ctx.Staff.AnyAsync(s => s.Id == id);
            if (!exists) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Owner")]
    public async Task<IActionResult> Delete(int id)
    {
        var staff = await _ctx.Staff.FindAsync(id);
        if (staff == null) return NotFound();

        _ctx.Staff.Remove(staff);
        await _ctx.SaveChangesAsync();

        return NoContent();
    }
}

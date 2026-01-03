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

public class CustomersController : ControllerBase
{
    private readonly AppDbContext _ctx;

    public CustomersController(AppDbContext ctx)
    {
        _ctx = ctx;
    }

    // GET: api/customers
    // MAUI mag de klantenlijst zonder login gebruiken (zoals bij appointments)
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Customer>>> GetAll()
    {
        var customers = await _ctx.Customers
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(customers);
    }

    // GET: api/customers/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Customer>> Get(int id)
    {
        var customer = await _ctx.Customers
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return NotFound();

        return customer;
    }

    // POST: api/customers
    // Alleen Admin mag klanten aanmaken via API
    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<Customer>> Post(Customer customer)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _ctx.Customers.Add(customer);
        await _ctx.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = customer.Id }, customer);
    }

    // PUT: api/customers/5
    [HttpPut("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Put(int id, Customer customer)
    {
        if (id != customer.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _ctx.Entry(customer).State = EntityState.Modified;

        try
        {
            await _ctx.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _ctx.Customers.AnyAsync(c => c.Id == id))
                return NotFound();

            throw;
        }

        return NoContent();
    }

    // DELETE: api/customers/5
    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _ctx.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null)
            return NotFound();

        _ctx.Customers.Remove(customer);
        await _ctx.SaveChangesAsync();

        return NoContent();
    }
}

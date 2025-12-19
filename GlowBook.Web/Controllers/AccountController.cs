using GlowBook.Model.Entities;
using GlowBook.Web.Models.Account;
using GlowBook.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace GlowBook.Web.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _email;

    public AccountController(SignInManager<ApplicationUser> signInManager,
                             UserManager<ApplicationUser> userManager,
                             IEmailService email)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _email = email;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (Request.Query["blocked"] == "1")
        {
            ViewBag.BlockedMessage = "Je account is geblokkeerd. Contacteer de beheerder.";
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);

        // Bestaat user? 
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError("", "Ongeldige login");
            return View(model);
        }

        // Email bevestiging verplicht 
        if (!user.EmailConfirmed)
        {
            ModelState.AddModelError("", "Bevestig eerst je e-mailadres voordat je kan inloggen.");
            return View(model);
        }

        // login via UserName 
        var result = await _signInManager.PasswordSignInAsync(
            userName: user.UserName!,
            password: model.Password,
            isPersistent: model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Ongeldige login");
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email, // identity login gebruikt UserName
            Email = model.Email,
            DisplayName = model.DisplayName,
            EmailConfirmed = false,       
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "Employee");

            // Email confirmatie token + link
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                action: nameof(ConfirmEmail),
                controller: "Account",
                values: new { userId = user.Id, token = token },
                protocol: Request.Scheme);

            var safeUrl = HtmlEncoder.Default.Encode(callbackUrl!);

            await _email.SendAsync(
                toEmail: user.Email!,
                subject: "Bevestig je e-mailadres (GlowBook)",
                htmlBody:
                    $"<p>Hallo {HtmlEncoder.Default.Encode(user.DisplayName)},</p>" +
                    $"<p>Klik <a href=\"{safeUrl}\">hier</a> om je e-mailadres te bevestigen.</p>");

            TempData["Message"] = "Registratie gelukt! Controleer je mailbox om je e-mail te bevestigen.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Description);

        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            return BadRequest();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();

        var result = await _userManager.ConfirmEmailAsync(user, token);

        TempData["Message"] = result.Succeeded
            ? "E-mail bevestigd. Je kan nu inloggen."
            : "E-mail bevestiging mislukt.";

        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}

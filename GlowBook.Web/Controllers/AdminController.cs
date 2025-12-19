using GlowBook.Model.Entities;
using GlowBook.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GlowBook.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

    public async Task<IActionResult> Users()
    {
        var users = _userManager.Users.ToList();
        var list = new List<UserListItemViewModel>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            list.Add(new UserListItemViewModel
            {
                Id = u.Id,
                Email = u.Email ?? "",
                DisplayName = u.DisplayName,
                EmailConfirmed = u.EmailConfirmed,
                IsActive = u.IsActive,
                Roles = roles.ToList()
            });
        }

        return View(list.OrderBy(x => x.Email).ToList());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (id == currentUserId)
        {
            TempData["Message"] = "Je kan jezelf niet blokkeren.";
            return RedirectToAction(nameof(Users));
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        TempData["Message"] = user.IsActive ? "Gebruiker geactiveert." : "Gebruiker geblokkeerd.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> EditRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
        var userRoles = await _userManager.GetRolesAsync(user);

        var vm = new EditUserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? "",
            DisplayName = user.DisplayName,
            Roles = allRoles.Select(r => new EditUserRolesViewModel.RoleCheckItem
            {
                RoleName = r,
                IsSelected = userRoles.Contains(r)
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoles(EditUserRolesViewModel vm)
    {
        var user = await _userManager.FindByIdAsync(vm.UserId);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        var selectedRoles = vm.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

        var toRemove = currentRoles.Where(r => !selectedRoles.Contains(r)).ToList();
        var toAdd = selectedRoles.Where(r => !currentRoles.Contains(r)).ToList();

        if (toRemove.Any())
            await _userManager.RemoveFromRolesAsync(user, toRemove);

        if (toAdd.Any())
            await _userManager.AddToRolesAsync(user, toAdd);

        TempData["Message"] = "Rollen bijgewerkt.";
        return RedirectToAction(nameof(Users));
    }
}

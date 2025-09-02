using System.Reflection;
using System.Security.Claims;
using Aiursoft.Template.Authorization;
using Aiursoft.Template.Entities;
using Aiursoft.Template.Models.RolesViewModels;
using Aiursoft.Template.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.Template.Controllers;

[Authorize(Roles = "Admin")]
public class RolesController(
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager)
    : Controller
{
    // GET: Roles
    public async Task<IActionResult> Index()
    {
        var roles = await roleManager.Roles
            .Select(role => new IdentityRoleWithCount
            {
                Role = role,
                UserCount = userManager.GetUsersInRoleAsync(role.Name!).Result.Count
            })
            .ToListAsync();
        return this.StackView(new IndexViewModel
        {
            Roles = roles
        });
    }

    // GET: Roles/Details/5
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        return this.StackView(new DetailsViewModel
        {
            Role = role
        });
    }

    // GET: Roles/Create
    public IActionResult Create()
    {
        return this.StackView(new CreateViewModel());
    }

    // POST: Roles/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            var role = new IdentityRole(model.RoleName!);
            var result = await roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return this.StackView(model);
    }

    // GET: Roles/Edit/5
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var model = new EditViewModel
        {
            Id = role.Id,
            RoleName = role.Name!
        };

        var existingClaims = await roleManager.GetClaimsAsync(role);

        var allPossibleClaims = typeof(AppClaims)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(fi => fi.FieldType == typeof(string) && fi.Name != nameof(AppClaims.Type))
            .Select(fi => (string)fi.GetValue(null)!)
            .ToList();

        foreach (var claimValue in allPossibleClaims)
        {
            model.Claims.Add(new RoleClaimViewModel
            {
                ClaimType = claimValue,
                IsSelected = existingClaims.Any(c => c.Type == AppClaims.Type && c.Value == claimValue)
            });
        }

        return this.StackView(model);
    }

    // POST: Roles/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            role.Name = model.RoleName;
            await roleManager.UpdateAsync(role);

            var existingClaims = await roleManager.GetClaimsAsync(role);

            // Remove unselected claims
            foreach (var existingClaim in existingClaims)
            {
                if (!model.Claims.Any(c => c.ClaimType == existingClaim.Value && c.IsSelected))
                {
                    await roleManager.RemoveClaimAsync(role, existingClaim);
                }
            }

            // Add newly selected claims
            foreach (var claimViewModel in model.Claims)
            {
                if (claimViewModel.IsSelected && existingClaims.All(c => c.Value != claimViewModel.ClaimType))
                {
                    await roleManager.AddClaimAsync(role, new Claim(AppClaims.Type, claimViewModel.ClaimType));
                }
            }

            return RedirectToAction(nameof(Index));
        }
        return this.StackView(model);
    }

    // GET: Roles/Delete/5
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        return this.StackView(new DeleteViewModel
        {
            Role = role
        });
    }

    // POST: Roles/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        await roleManager.DeleteAsync(role);
        return RedirectToAction(nameof(Index));
    }
}

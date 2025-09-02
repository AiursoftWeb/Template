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

        return this.StackView(new EditViewModel
        {
            Id = role.Id,
            RoleName = role.Name!
        });
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
            try
            {
                var role = await roleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return NotFound();
                }
                role.Name = model.RoleName;
                await roleManager.UpdateAsync(role);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RoleExists(model.Id))
                {
                    return NotFound();
                }
                throw;
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

    private async Task<bool> RoleExists(string id)
    {
        return await roleManager.RoleExistsAsync(await roleManager.FindByIdAsync(id).ContinueWith(t => t.Result?.Name ?? string.Empty));
    }
}

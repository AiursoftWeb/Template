using Aiursoft.Template.Entities;
using Aiursoft.Template.Models.UsersViewModels;
using Aiursoft.Template.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Aiursoft.Template.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController(
    RoleManager<IdentityRole> roleManager,
    UserManager<User> userManager,
    TemplateDbContext context)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        var allUsers = await context.Users.ToListAsync();
        var usersWithRoles = new List<UserWithRolesViewModel>();
        foreach (var user in allUsers)
        {
            usersWithRoles.Add(new UserWithRolesViewModel
            {
                User = user,
                Roles = await userManager.GetRolesAsync(user)
            });
        }

        return this.StackView(new IndexViewModel
        {
            Users = usersWithRoles
        });
    }

    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return this.StackView(new DetailsViewModel
        {
            User = user
        });
    }

    public IActionResult Create()
    {
        return this.StackView(new CreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateViewModel newTeacher)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = newTeacher.UserName,
                Email = newTeacher.Email,
            };
            var result = await userManager.CreateAsync(user, newTeacher.Password!);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return this.StackView(newTeacher);
            }

            return RedirectToAction(nameof(Index));
        }
        return this.StackView(newTeacher);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null) return NotFound();
        var user = await context.Users.FindAsync(id);
        if (user == null) return NotFound();

        // 2. 获取用户当前拥有的所有角色
        var userRoles = await userManager.GetRolesAsync(user);

        // 3. 获取系统中的所有角色
        var allRoles = await roleManager.Roles.ToListAsync();

        var model = new EditViewModel
        {
            Id = id,
            Email = user.Email!,
            UserName = user.UserName!,
            Password = "you-cant-read-it",
            // 4. 构建视图模型所需的数据
            AllRoles = allRoles.Select(role => new UserRoleViewModel
            {
                RoleName = role.Name!,
                // 如果用户拥有该角色，则 IsSelected 为 true
                IsSelected = userRoles.Contains(role.Name!)
            }).ToList()
        };

        return this.StackView(model);
    }

    // POST: Users/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var userInDb = await userManager.FindByIdAsync(id);
            if (userInDb == null) return NotFound();

            userInDb.Email = model.Email;
            userInDb.UserName = model.UserName;
            // 更新用户信息
            await userManager.UpdateAsync(userInDb);

            // 5. 更新用户的角色
            var userCurrentRoles = await userManager.GetRolesAsync(userInDb);
            var selectedRoles = model
                .AllRoles
                .Where(r => r.IsSelected)
                .Select(r => r.RoleName)
                .ToArray();

            // 5.1 计算需要添加的角色
            var rolesToAdd = selectedRoles.Except(userCurrentRoles);
            await userManager.AddToRolesAsync(userInDb, rolesToAdd);

            // 5.2 计算需要移除的角色
            var rolesToRemove = userCurrentRoles.Except(selectedRoles);
            await userManager.RemoveFromRolesAsync(userInDb, rolesToRemove);

            // 6. 处理密码重置
            if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != "you-cant-read-it")
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(userInDb);
                await userManager.ResetPasswordAsync(userInDb, token, model.Password);
            }

            return RedirectToAction(nameof(Index));
        }
        return this.StackView(model);
    }

    // GET: Users/Delete/5
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return this.StackView(new DeleteViewModel
        {
            User = user,
        });
    }

    // POST: Teachers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        await userManager.RemoveFromRoleAsync(user, "Admin");
        context.Users.Remove(user);

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

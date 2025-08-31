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
    UserManager<User> userManager,
    TemplateDbContext context)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = await context.Users.ToListAsync();
        return this.StackView(new IndexViewModel
        {
            Users = users
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

    // GET: Teachers/Edit/5
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var teacher = await context.Users.FindAsync(id);
        if (teacher == null)
        {
            return NotFound();
        }

        return this.StackView(new EditViewModel
        {
            Id = id,
            Email = teacher.Email!,
            IsAdmin = await userManager.IsInRoleAsync(teacher, "Admin"),
            UserName = teacher.UserName!,
            Password = "you-cant-read-it",
        });
    }

    // POST: Teachers/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                var teacherInDb = await context.Users.FindAsync(id);
                if (teacherInDb == null)
                {
                    return NotFound();
                }

                teacherInDb.Email = model.Email;
                teacherInDb.UserName = model.UserName;
                if (model.IsAdmin)
                {
                    await userManager.AddToRoleAsync(teacherInDb, "Admin");
                }
                else
                {
                    await userManager.RemoveFromRoleAsync(teacherInDb, "Admin");
                }

                context.Update(teacherInDb);
                await context.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(teacherInDb);
                    await userManager.ResetPasswordAsync(teacherInDb, token, model.Password);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(model.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return this.StackView(model);
    }

    // GET: Teachers/Delete/5
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

    private bool TeacherExists(string id)
    {
        return context.Users.Any(e => e.Id == id);
    }
}

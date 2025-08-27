using Aiursoft.Template.Entities;
using Aiursoft.Template.Models.UsersViewModels;
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
          return View(await context.Users.ToListAsync());
    }

    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var teacher = await context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (teacher == null)
        {
            return NotFound();
        }

        return View(teacher);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTeacherAddressModel newTeacher)
    {
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = newTeacher.Email,
                Email = newTeacher.Email,
            };
            var result = await userManager.CreateAsync(user, newTeacher.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(newTeacher);
            }

            return RedirectToAction(nameof(Index));
        }
        return View(newTeacher);
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

        return View(new EditTeacherViewModel(HttpContext)
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
    public async Task<IActionResult> Edit(string id, EditTeacherViewModel model)
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
        return View(model);
    }

    // GET: Teachers/Delete/5
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var teacher = await context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (teacher == null)
        {
            return NotFound();
        }

        return View(teacher);
    }

    // POST: Teachers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var teacher = await context.Users.FindAsync(id);
        if (teacher == null)
        {
            return Problem("Entity set 'ApplicationDbContext.Teachers'  is null.");
        }

        await userManager.RemoveFromRoleAsync(teacher, "Admin");
        context.Users.Remove(teacher);

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TeacherExists(string id)
    {
        return context.Users.Any(e => e.Id == id);
    }
}

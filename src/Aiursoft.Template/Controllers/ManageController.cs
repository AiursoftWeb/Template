using Aiursoft.Template.Entities;
using Aiursoft.Template.Models.ManageViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Controllers;

[Authorize]
public class ManageController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILoggerFactory loggerFactory)
    : Controller
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<ManageController>();

    //
    // GET: /Manage/Index
    [HttpGet]
    public IActionResult Index(ManageMessageId? message = null)
    {
        ViewData["StatusMessage"] =
            message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
            : message == ManageMessageId.Error ? "An error has occurred."
            : "";

        var model = new IndexViewModel();
        return this.StackView(model);
    }

    //
    // GET: /Manage/ChangePassword
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return this.StackView(new ChangePasswordViewModel());
    }

    //
    // POST: /Manage/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return this.StackView(model);
        }
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            var result = await userManager.ChangePasswordAsync(user, model.OldPassword!, model.NewPassword!);
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(3, "User changed their password successfully");
                return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return this.StackView(model);
        }
        return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
    }

    #region Helpers

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    public enum ManageMessageId
    {
        ChangePasswordSuccess,
        Error
    }

    private Task<User?> GetCurrentUserAsync()
    {
        return userManager.GetUserAsync(HttpContext.User);
    }

    #endregion
}

using Aiursoft.Template.Configuration;
using Aiursoft.Template.Entities;
using Aiursoft.Template.Models.ManageViewModels;
using Aiursoft.Template.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Aiursoft.Template.Controllers;

[Authorize]
public class ManageController(
    IStringLocalizer<ManageController> localizer,
    IOptions<AppSettings> appSettings,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<ManageController> logger)
    : Controller
{
    //
    // GET: /Manage/Index
    [HttpGet]
    public IActionResult Index(ManageMessageId? message = null)
    {
        ViewData["StatusMessage"] =
            message == ManageMessageId.ChangePasswordSuccess ? localizer["Your password has been changed."]
            : message == ManageMessageId.Error ? localizer["An error has occurred."]
            : "";

        var model = new IndexViewModel();
        return this.StackView(model);
    }

    //
    // GET: /Manage/ChangePassword
    [HttpGet]
    public IActionResult ChangePassword()
    {
        if (appSettings.Value.OIDCEnabled)
        {
            return BadRequest("Local password is disabled when OIDC authentication is enabled.");
        }
        return this.StackView(new ChangePasswordViewModel());
    }

    //
    // POST: /Manage/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (appSettings.Value.OIDCEnabled)
        {
            return BadRequest("Local password is disabled when OIDC authentication is enabled.");
        }
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
                logger.LogInformation(3, "User changed their password successfully");
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

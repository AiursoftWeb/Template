using Aiursoft.Template.Entities;
using Aiursoft.Template.Models.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.Template.Controllers;

[Authorize]
public class AccountController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILoggerFactory loggerFactory)
    : Controller
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AccountController>();

    //
    // GET: /Account/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return this.StackView(new LoginViewModel());
    }

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var possibleUser = await userManager.FindByEmailAsync(model.EmailOrUserName!);
            if (possibleUser == null)
            {
                possibleUser = await userManager.FindByNameAsync(model.EmailOrUserName!);
            }

            if (possibleUser == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return this.StackView(new LoginViewModel());
            }

            var result = await signInManager.PasswordSignInAsync(possibleUser, model.Password!, true, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _logger.LogInformation(1, "User logged in");
                return RedirectToLocal(returnUrl ?? "/");
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(2, "User account locked out");
                return this.StackView(new LockoutViewModel());
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return this.StackView(model);
            }
        }

        // If we got this far, something failed, redisplay form
        return this.StackView(model);
    }

    //
    // GET: /Account/Register
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return this.StackView(new RegisterViewModel());
    }

    //
    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var user = new User
            {
                UserName = model.Email!.Split('@')[0],
                Email = model.Email,
            };
            var result = await userManager.CreateAsync(user, model.Password!);
            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation(3, "User created a new account with password");
                return RedirectToLocal(returnUrl ?? "/");
            }
            AddErrors(result);
        }

        // If we got this far, something failed, redisplay form
        return this.StackView(model);
    }

    //
    // POST: /Account/LogOff
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOff()
    {
        await signInManager.SignOutAsync();
        _logger.LogInformation(4, "User logged out");
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    #region Helpers

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }

    #endregion
}

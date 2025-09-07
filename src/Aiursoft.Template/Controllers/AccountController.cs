using Aiursoft.Template.Configuration;
using Aiursoft.Template.Entities;
using Aiursoft.Template.Models.AccountViewModels;
using Aiursoft.Template.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aiursoft.Template.Controllers;

[Authorize]
public class AccountController(
    IOptions<AppSettings> appSettings,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILoggerFactory loggerFactory)
    : Controller
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AccountController>();
    private readonly AppSettings _appSettings = appSettings.Value;

    // GET: /Account/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_appSettings.OIDCEnabled)
        {
            var provider = OpenIdConnectDefaults.AuthenticationScheme;
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return this.StackView(new LoginViewModel());
    }

    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (_appSettings.OIDCEnabled)
        {
            return BadRequest("Local login is disabled when OIDC authentication is enabled.");
        }

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

            var result =
                await signInManager.PasswordSignInAsync(possibleUser, model.Password!, true, lockoutOnFailure: true);
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

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return this.StackView(model);
    }

    // GET: /Account/Register
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        // 如果 OIDC 启用，或本地注册被禁用，则不允许访问
        if (_appSettings.OIDCEnabled || !_appSettings.Local.AllowRegister)
        {
            return BadRequest("Registration is not allowed in the current configuration.");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return this.StackView(new RegisterViewModel());
    }

    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        if (_appSettings.OIDCEnabled || !_appSettings.Local.AllowRegister)
        {
            return BadRequest("Registration is not allowed in the current configuration.");
        }

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

        return this.StackView(model);
    }

    public async Task<IActionResult> LogOff()
    {
        if (_appSettings.OIDCEnabled)
        {
            _logger.LogInformation(4, "User logged out with OIDC.");
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            return SignOut(properties, IdentityConstants.ApplicationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        await signInManager.SignOutAsync();
        _logger.LogInformation(4, "User logged out locally.");
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError != null)
        {
            ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
            return RedirectToAction(nameof(Login));
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
            isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
            return RedirectToLocal(returnUrl ?? "/");
        }

        if (result.IsLockedOut)
        {
            return this.StackView(new LockoutViewModel());
        }
        else
        {
            ModelState.AddModelError(string.Empty,
                "Failed to associate external login. The user may not exist locally.");
            return RedirectToAction(nameof(Login));
        }
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

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    #endregion
}

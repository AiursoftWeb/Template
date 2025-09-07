using Aiursoft.Template.Configuration;
using Aiursoft.Template.Entities;
using Aiursoft.Template.Models.AccountViewModels;
using Aiursoft.Template.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        // 如果 OIDC 启用，不显示登录页，直接挑战 OIDC Provider
        if (_appSettings.OIDCEnabled)
        {
            var properties = new AuthenticationProperties { RedirectUri = returnUrl ?? "/" };
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        // 否则，显示本地登录页
        ViewData["ReturnUrl"] = returnUrl;
        return this.StackView(new LoginViewModel());
    }

    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        // 如果 OIDC 启用，本地登录 POST 请求是无效的
        if (_appSettings.OIDCEnabled)
        {
            return BadRequest("Local login is disabled when OIDC authentication is enabled.");
        }

        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            // ... (原有的本地登录逻辑保持不变)
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
        // 如果 OIDC 启用，或本地注册被禁用，则不允许注册
        if (_appSettings.OIDCEnabled || !_appSettings.Local.AllowRegister)
        {
            return BadRequest("Registration is not allowed in the current configuration.");
        }

        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            // ... (原有的本地注册逻辑保持不变)
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

    // 优化后的 LogOff 方法
    public async Task<IActionResult> LogOff()
    {
        if (_appSettings.OIDCEnabled)
        {
            // 对于 OIDC，需要同时登出本地 Cookie 和 OIDC Provider
            _logger.LogInformation(4, "User logged out with OIDC.");
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            // 同时指定两个认证方案，实现联合登出
            return SignOut(properties, CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
        }

        // 对于本地登录，只需登出本地 Cookie
        await signInManager.SignOutAsync();
        _logger.LogInformation(4, "User logged out locally.");
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
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
    #endregion
}

using Aiursoft.Template.Configuration;
using Aiursoft.Template.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Aiursoft.Template.Services;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddTemplateAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>()!;

        // 1. AddIdentity 依然是起点，它会“隐式地”注册基础的认证服务和方案
        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
            })
            .AddEntityFrameworkStores<TemplateDbContext>()
            .AddDefaultTokenProviders();

        if (appSettings.OIDCEnabled)
        {
            // 配置 OIDC 认证
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logoff";
                    options.AccessDeniedPath = "/Home/Index";
                })
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = appSettings.OIDC.Authority;
                    options.ClientId = appSettings.OIDC.ClientId;
                    options.ClientSecret = appSettings.OIDC.ClientSecret;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.MapInboundClaims = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = appSettings.OIDC.UsernamePropertyName,
                        RoleClaimType = appSettings.OIDC.RolePropertyName
                    };

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var userManager =
                                context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                            var signInManager = context.HttpContext.RequestServices
                                .GetRequiredService<SignInManager<User>>();
                            var roleManager = context.HttpContext.RequestServices
                                .GetRequiredService<RoleManager<IdentityRole>>();
                            var principal = context.Principal!;

                            var username = principal.FindFirst(appSettings.OIDC.UsernamePropertyName)?.Value;
                            var email = principal.FindFirst(appSettings.OIDC.EmailPropertyName)?.Value;

                            if (string.IsNullOrEmpty(username))
                            {
                                context.Fail("Could not find username claim.");
                                return;
                            }

                            if (string.IsNullOrEmpty(email))
                            {
                                context.Fail("Could not find email claim.");
                                return;
                            }

                            var localUser = await userManager.FindByNameAsync(username);
                            if (localUser is not null)
                            {
                                if (!string.IsNullOrWhiteSpace(email) &&
                                    !string.Equals(localUser.Email, email, StringComparison.OrdinalIgnoreCase))
                                {
                                    localUser.Email = email;
                                    await userManager.UpdateAsync(localUser);
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(email))
                            {
                                localUser = await userManager.FindByEmailAsync(email);
                                if (localUser is not null)
                                {
                                    await userManager.SetUserNameAsync(localUser, username);
                                }
                            }

                            if (localUser is null)
                            {
                                localUser = new User { UserName = username, Email = email };
                                var createUserResult = await userManager.CreateAsync(localUser);
                                if (!createUserResult.Succeeded)
                                {
                                    context.Fail(
                                        $"创建本地用户失败: {string.Join(", ", createUserResult.Errors.Select(e => e.Description))}");
                                    return;
                                }

                                if (!string.IsNullOrWhiteSpace(appSettings.DefaultRoleForNewUser))
                                {
                                    if (await roleManager.RoleExistsAsync(appSettings.DefaultRoleForNewUser))
                                    {
                                        await userManager.AddToRoleAsync(localUser, appSettings.DefaultRoleForNewUser);
                                    }
                                }
                            }

                            var oidcRoles = principal.FindAll(appSettings.OIDC.RolePropertyName).Select(c => c.Value)
                                .ToHashSet();
                            var localRoles = (await userManager.GetRolesAsync(localUser)).ToHashSet();
                            var rolesToAdd = oidcRoles.Except(localRoles);
                            foreach (var roleName in rolesToAdd)
                            {
                                if (!await roleManager.RoleExistsAsync(roleName))
                                {
                                    await roleManager.CreateAsync(new IdentityRole(roleName));
                                }

                                await userManager.AddToRoleAsync(localUser, roleName);
                            }

                            var rolesToRemove = localRoles.Except(oidcRoles).ToArray();
                            if (rolesToRemove.Any())
                            {
                                await userManager.RemoveFromRolesAsync(localUser, rolesToRemove);
                            }

                            // 1. 像之前一样，创建 Identity 框架认可的 Principal
                            var newPrincipal = await signInManager.CreateUserPrincipalAsync(localUser);

                            // 2. 手动、显式地使用主应用 Cookie 方案来签发登录 Cookie
                            // 这是 SignInManager.SignInAsync 的底层核心调用
                            await context.HttpContext.SignInAsync(
                                IdentityConstants.ApplicationScheme,
                                newPrincipal,
                                context.Properties); // 传递 OIDC 的属性，如 isPersistent
                            //await signInManager.SignInAsync(localUser, context.Properties!);

                            // 3. 告诉 OIDC 处理器，我们已经完全处理了这次认证响应，你不需要再做任何事了
                            context.HandleResponse();

                            // 4. 手动处理成功后的重定向
                            context.Response.Redirect(context.Properties!.RedirectUri ?? "/");
                        }
                    };
                });
        }

        return services;
    }
}

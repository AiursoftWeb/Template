using System.Security.Claims;
using Aiursoft.Template.Configuration;
using Aiursoft.Template.Entities;
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

        // 这部分对两种认证类型都是通用的
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

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = appSettings.OIDC.RolePropertyNames
                    };

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<User>>();
                            var roleManager = context.HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();
                            var principal = context.Principal!;

                            // ==================== 新的用户同步逻辑 开始 ====================

                            // 1. 从 OIDC claims 中提取关键信息
                            var username = principal.FindFirst("name")?.Value;
                            var email = principal.FindFirst(ClaimTypes.Email)?.Value;

                            if (string.IsNullOrEmpty(username))
                            {
                                context.Fail("OIDC 令牌中未找到用户名 ('name') claim。");
                                return;
                            }

                            User? localUser;

                            // 2. 第一步：优先按用户名查找
                            localUser = await userManager.FindByNameAsync(username);
                            if (localUser is not null)
                            {
                                // 找到了！如果 Email 不一致，则更新本地 Email
                                if (!string.IsNullOrWhiteSpace(email) &&
                                    !string.Equals(localUser.Email, email, StringComparison.OrdinalIgnoreCase))
                                {
                                    localUser.Email = email;
                                    await userManager.UpdateAsync(localUser);
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(email))
                            {
                                // 3. 第二步：如果按用户名找不到，且 Email 存在，则按 Email 查找
                                localUser = await userManager.FindByEmailAsync(email);
                                if (localUser is not null)
                                {
                                    // 找到了！说明用户在 OIDC 端修改了用户名，同步更新本地用户名
                                    await userManager.SetUserNameAsync(localUser, username);
                                }
                            }

                            // 4. 第三步：如果都找不到，则创建新用户
                            if (localUser is null)
                            {
                                localUser = new User
                                {
                                    UserName = username,
                                    Email = email
                                };
                                await userManager.CreateAsync(localUser); // OIDC 用户无需密码

                                // 如果配置了，则分配默认角色
                                if (!string.IsNullOrWhiteSpace(appSettings.DefaultRoleForNewUser))
                                {
                                    if (await roleManager.RoleExistsAsync(appSettings.DefaultRoleForNewUser))
                                    {
                                        await userManager.AddToRoleAsync(localUser, appSettings.DefaultRoleForNewUser);
                                    }
                                }
                            }

                            // ==================== 新的用户同步逻辑 结束 ====================

                            // 5. 同步角色 (这部分逻辑不变)
                            var oidcRoles = principal.FindAll(appSettings.OIDC.RolePropertyNames).Select(c => c.Value).ToHashSet();
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

                            // 6. 为应用程序会话创建新的 claims identity (这部分逻辑不变)
                            var finalRoles = await userManager.GetRolesAsync(localUser);
                            var newIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                            newIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, localUser.Id));
                            newIdentity.AddClaim(new Claim(ClaimTypes.Name, localUser.UserName!));
                            foreach (var role in finalRoles)
                            {
                                newIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                            }

                            // 用我们自己的 principal 替换 OIDC 的 principal
                            context.Principal = new ClaimsPrincipal(newIdentity);
                        }
                    };
                });
        }
        else // 回退到 "Local" 本地认证
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logoff";
                    options.AccessDeniedPath = "/Home/Index";
                });
        }

        return services;
    }
}

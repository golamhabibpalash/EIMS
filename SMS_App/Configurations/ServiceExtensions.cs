using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SMS_App.Configurations;

public static class ServiceExtensions
{
    public static void AddSessionConfiguration(this IServiceCollection services)
    {
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(60);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
    }

    public static void ConfigureApplicationCookie(IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.AccessDeniedPath = new PathString("/Accounts/AccessDenied");
            options.Cookie.Name = "Cookie";
            options.Cookie.HttpOnly = true;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
            options.LoginPath = new PathString("/Accounts/Login");
            options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
            options.SlidingExpiration = true;
        });

    }
}

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Formula.SimpleMembership
{
    public static class SimpleMembershipConfiguration
    {
        // https://stackoverflow.com/questions/42030137/suppress-redirect-on-api-urls-in-asp-net-core/42030138#42030138
        static Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceRedirector(HttpStatusCode statusCode,
        Func<RedirectContext<CookieAuthenticationOptions>, Task> existingRedirector) =>
        context =>
        {
            context.Response.Headers["Location"] = context.RedirectUri;
            context.Response.StatusCode = (int)statusCode;
            return Task.CompletedTask;
            /*
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = (int)statusCode;
                return Task.CompletedTask;
            }
            return existingRedirector(context);
            */
        };

        public static IServiceCollection AddSimpleMembership(this IServiceCollection services, IConfiguration configuration, String migrationsAssembly, String connectionName = "DefaultConnection")
        {

            bool useInMemoryAuthProvider = bool.Parse(configuration.GetValue<String>("InMemoryAuthProvider"));

            if (useInMemoryAuthProvider) {
                services.AddDbContext<SimpleMembershipDbContext>(options => {
                    options.UseInMemoryDatabase(connectionName);
                });
            }
            else {
                services.AddDbContext<SimpleMembershipDbContext>(options => {
                    options.UseSqlServer(configuration.GetConnectionString(connectionName),
                        optionsBuilder => 
                        optionsBuilder.MigrationsAssembly(migrationsAssembly));
                });
            }

            services.AddIdentity<ApplicationUser, IdentityRole> ()
                .AddEntityFrameworkStores<SimpleMembershipDbContext> ()
                .AddDefaultTokenProviders();

/*
            services.ConfigureApplicationCookie (options => {
                options.Events.OnRedirectToLogin = context => {
                    context.Response.Headers["Location"] = context.RedirectUri;
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context => {
                    context.Response.Headers["Location"] = context.RedirectUri;
                    context.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            });

            services.AddScoped<IDbInitializer, DbInitializer> ();
*/

            return services;
        }
    }
}

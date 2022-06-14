using AuthPracticeLibrary;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;

namespace UserHasMongoStoredAccount
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("Auth").AddCookie("Auth", cookieConfig =>
                {
                    cookieConfig.LoginPath = "/Home/Index";
                    cookieConfig.Cookie.Name = "Auth.coooookie";
                    cookieConfig.AccessDeniedPath = "/Home/AccessDenied";
                });

            services.AddAuthorization(authConfig =>
            {
                authConfig.AddPolicy("Test Policy", policyBuilder =>
                {
                    policyBuilder.RequireClaim(ClaimTypes.Name);
                    string[] rolesArray = { "Admin", "Dev" };
                    policyBuilder.RequireRole(roles: rolesArray);
                });
            });

            services.AddControllersWithViews();

            services.AddSingleton<MongoCRUD>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

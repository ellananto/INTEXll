using INTEXll.Data;
using INTEXll.Models;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using INTEXll.Models.RoleModel;
using Microsoft.ML.OnnxRuntime;
using System.IO;

namespace INTEXll
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }
        private readonly IWebHostEnvironment _env;
        
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var secrets = new ConfigurationBuilder().AddJsonFile("secrets.json", optional: true, reloadOnChange: true).Build();
            var connectionstring = Configuration.GetConnectionString("Database1Connection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<burialContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("Database1Connection")));

            services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddDefaultUI()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddSingleton<InferenceSession>(
              new InferenceSession(Path.Combine(_env.ContentRootPath, "wwwroot", "modelagain.onnx"))
            );

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential 
                // cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                // requires using Microsoft.AspNetCore.Http;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 3;
            });
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
                options.ExcludedHosts.Add("example.com");
                options.ExcludedHosts.Add("www.example.com");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();
            app.Use(async (context, next) => {
                context.Response.Headers.Add("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://code.jquery.com https://cdn.jsdelivr.net; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "font-src 'self' data:; " +
                    "img-src 'self' data:; " +
                    "frame-src 'self'");

                await next();
            });
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute("id", "{controller}/{action}/{id?}", new { Controller = "Home", action = "MoreInfo" });
                endpoints.MapControllerRoute("typepage", "{area}/P{pageNum}", new { Controller = "Home", action = "Burial" });
                endpoints.MapControllerRoute("ellaid", "{action}/{ellaid}", new { Controller = "Home", action = "MoreInfo" });
                endpoints.MapControllerRoute("Paging", "Page{pageNum}", new { controller = "Home", action = "Burials" });
                endpoints.MapControllerRoute("type", "{area}", new { Controller = "Home", action = "Burials", pageNum = 1 });
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }
    }
}
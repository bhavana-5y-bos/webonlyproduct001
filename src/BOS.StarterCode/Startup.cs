using BOS.Auth.Client.ServiceExtension;
using BOS.IA.Client.ServiceExtension;
using BOS.StarterCode.Helpers;
using BOS.StarterCode.Policy.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BOS.StarterCode
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddHttpClient();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            AddCors(services);

            //Configuring BOS Services
            string bosServiceURL = Configuration["BOS:ServiceBaseURL"];
            string bosAPIkey = Configuration["BOS:APIkey"];
            services.AddBOSAuthClient(bosAPIkey, bosServiceURL + Configuration["BOS:AuthRelativeURL"]); //Adding BOS Auth Client to the Service and providing the enpoint URL
            services.AddBOSIAClient(bosAPIkey, bosServiceURL + Configuration["BOS:IARelativeURL"]); //Adding BOS Information Architecture Client to the Service and providing the 
            services.AddBOSEmailClient(bosAPIkey, bosServiceURL + Configuration["BOS:EmailRelativeURL"]);

            //Configuring Auth Policy
            ConfigureAuthPolicies(services, Configuration);
            services.AddSingleton<IAuthorizationHandler, AdminOnlyHandler>();
            services.AddSingleton<IAuthorizationHandler, IsAuthenticatedHandler>();

            services.AddTransient<IEmailSender>(e => new EmailHelper (Configuration["SendGrid:From"], Configuration["SendGrid:ApiKey"]));

            services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
            services.AddSession(
                s =>
                {
                    s.IdleTimeout = TimeSpan.FromMinutes(30);
                    s.Cookie.HttpOnly = true;
                }
            );
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseAuthentication();
            var cachePeriod = env.IsDevelopment() ? "600" : "604800";
            app.UseStaticFiles(
               new StaticFileOptions
               {
                   OnPrepareResponse = ctx =>
                   {                     
                       ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                   }
               });

            app.UseCookiePolicy();
            app.UseSession();
            app.UseCors("CorsPolicy");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Auth}/{action=Index}/{id?}");
            });
        }

        private void ConfigureAuthPolicies(IServiceCollection services, IConfiguration configuration)
        {
            // configure jwt authentication  
            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Auth");
                options.LoginPath = new PathString("/Auth");
                options.LogoutPath = new PathString("/Auth");
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.Requirements.Add(new IsAuthenticatedRequirement());
                    policy.Requirements.Add(new AdminOnlyRequirement(new string[] { "Admin", "Super Admin" }));
                });
                options.AddPolicy("IsAuthenticated", policy =>
                {
                    policy.Requirements.Add(new IsAuthenticatedRequirement());
                });
            });
        }

        private void AddCors(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });
        }
    }
}


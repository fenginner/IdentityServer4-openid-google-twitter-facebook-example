using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServer
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup( IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var builder = services.AddIdentityServer()
               .AddInMemoryIdentityResources(Config.Ids)
               .AddInMemoryApiResources(Config.Apis)
               .AddDeveloperSigningCredential()
               .AddInMemoryClients(Config.Clients)
               .AddTestUsers(TestUsers.Users);

            services.AddAuthentication()
                      .AddGoogle("Google", options =>
                      {
                          options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                          options.ClientId = _config["Authentication:Google:ClientId"];
                          options.ClientSecret = _config["Authentication:Google:ClientSecret"];

                      })
                      .AddFacebook(facebookOptions =>
                      {
                          facebookOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                          facebookOptions.AppId = _config["Authentication:Facebook:AppId"];
                          facebookOptions.AppSecret = _config["Authentication:Facebook:AppSecret"];
                      })
                      .AddTwitter(twitterOptions =>
                      {
                          twitterOptions.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                          twitterOptions.ConsumerKey = _config["Authentication:Twitter:CustomerKey"];
                          twitterOptions.ConsumerSecret = _config["Authentication:Twitter:CustomerSecret"];
                      });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();               
            });
        }
    }
}

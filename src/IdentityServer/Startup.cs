using IdentityServer.Authorization.Role;
using IdentityServer.Authorization.User;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Linq;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer.DbContext;

namespace IdentityServer
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = _config["ConnectionStrings:Default"];
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            //Add context by default 
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(_config["ConnectionStrings:Default"]);
            }) ;
            

            //config context for Asp.Net Identity
            services.AddIdentity<ApplicationUser, IdentityRoleApp>()
                            .AddEntityFrameworkStores<ApplicationDbContext>()
                            .AddDefaultTokenProviders();

            services.AddMvc();

       

            var builder = services.AddIdentityServer()
                            .AddDeveloperSigningCredential()
                            .AddTestUsers(TestUsers.Users)
                             // this adds the config data from DB (clients, resources)
                             .AddConfigurationStore(options =>
                             {
                                 options.ConfigureDbContext = builder =>
                                      builder.UseSqlServer(connectionString,
                                       sql => sql.MigrationsAssembly(migrationsAssembly));
                             })
                            // this adds the operational data from DB (codes, tokens, consents)
                            .AddOperationalStore(options =>
                            {
                                options.ConfigureDbContext = builder =>
                                    builder.UseSqlServer(connectionString,
                                        sql => sql.MigrationsAssembly(migrationsAssembly));

                                // this enables automatic token cleanup. this is optional.
                                options.EnableTokenCleanup = true;
                                options.TokenCleanupInterval = 180;
                            })
                            .AddAspNetIdentity<ApplicationUser>();
         

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
            
            InitializeDatabase(app);
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


        private void InitializeDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                //migrating in relation of the operational data from DB (codes, tokens, consents)
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                //migrating all IdentityServer need. this adds  data from DB (clients, resources)
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.Ids)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.Apis)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                var context2 = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //migrating all entities that application need.
                context2.Database.Migrate();
            }
        }
    }
}
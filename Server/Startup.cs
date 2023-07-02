using System.IdentityModel.Tokens.Jwt;
using EventsManager.Server.Data;
using EventsManager.Server.Handlers.Queries.Users.GetMyUser;
using EventsManager.Server.Models;
using EventsManager.Server.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace EventsManager.Server;

public class Startup {
    
    public IConfiguration configRoot {
        get;
    }
    
    public Startup(IConfiguration configuration) {
        configRoot = configuration;
    }
    
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
        // Add services to the container.
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddDefaultIdentity<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 3;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredUniqueChars = 0;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        var issuer = configuration.GetSection("IdentityServer")["IssuerUri"];
        services.AddIdentityServer(options =>
            { 
                options.IssuerUri = issuer;
            })
            .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(opt => 
            {
                opt.IdentityResources["openid"].UserClaims.Add("role");
                opt.ApiResources.Single().UserClaims.Add("role");
            });

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("role");
        
        var googleAuthSection = configuration.GetSection("GoogleAuth");
        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.ClientId = googleAuthSection["Settings:ClientId"];
                options.ClientSecret = googleAuthSection["Settings:ClientSecret"];
            })
            .AddIdentityServerJwt();

        services.AddControllersWithViews();
        services.AddRazorPages();
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetMyUserQueryHandler).Assembly));
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblyContaining<GetMyUserQueryHandler>();
        });
        
        var blobStorageSection = configuration.GetSection("BlobStorage");
        services.Configure<BlobStorageSettings>(blobStorageSection);    
    }
    
    public void Configure(WebApplication app, IWebHostEnvironment env, IServiceProvider services) {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();    
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        
        app.UseDeveloperExceptionPage();
        
        StripeConfiguration.ApiKey = "sk_test_51NIGzHKiJO2GrIfAqD6y7dOzdabQBMgaEYtqR5DpoqUpKv3fJFku07qPxKzYJ8kIpppp733PHISdvibsXZbg2xwb00o6fnTu5j";
        
        app.UseHttpsRedirection();

        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseIdentityServer();
        app.UseAuthorization();
        
        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");
        
        app.Run();
    }
}

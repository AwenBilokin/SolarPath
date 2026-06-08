using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;
using SolarPath.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opts =>
{
    opts.Password.RequiredLength = 6;
    opts.Password.RequireNonAlphanumeric = false;
    opts.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(opts => opts.LoginPath = "/Account/Login");

builder.Services.AddScoped<IRouteService,   RouteService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Migrate + Seed
using (var scope = app.Services.CreateScope())
{
    var db   = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var rm   = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var um   = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await db.Database.MigrateAsync();

    foreach (var role in new[] { "Administrator", "Guide", "Tourist" })
        if (!await rm.RoleExistsAsync(role)) await rm.CreateAsync(new IdentityRole(role));

    const string adminEmail = "admin@solarpath.ua";
    if (await um.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser { FirstName = "Admin", LastName = "SolarPath",
            Email = adminEmail, UserName = adminEmail };
        await um.CreateAsync(admin, "Admin123!");
        await um.AddToRoleAsync(admin, "Administrator");
    }

    await SeedData.SeedRoutesAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

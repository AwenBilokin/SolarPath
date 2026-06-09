using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using SolarPath.Web.Data;
using SolarPath.Web.Models;
using SolarPath.Web.Services;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

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

// Для Railway — forwarded headers щоб HTTPS працював правильно
builder.Services.Configure<ForwardedHeadersOptions>(opts =>
{
    opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    opts.KnownNetworks.Clear();
    opts.KnownProxies.Clear();
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Migrate + Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var rm = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var um = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
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

// Forwarded headers ПЕРЕД усім іншим
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Прибрали UseHttpsRedirection — Railway сам обробляє SSL
app.UseStaticFiles();

// Забороняємо кешування сторінок з формами — виправляє помилку antiforgery при поверненні назад
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

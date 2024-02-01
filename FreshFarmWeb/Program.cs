using Microsoft.AspNetCore.Identity;
using FreshFarmWeb.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AuthDbContext>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AuthDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(config =>
{
	config.ExpireTimeSpan = TimeSpan.FromMinutes(20);
	config.LoginPath = "/login";
});

builder.Services.AddDataProtection();


// Session Timeout
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(20);
	options.Cookie.IsEssential = true;
});

builder.Services.AddLogging();

builder.Services.Configure<IdentityOptions>(options =>
{
	options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
	options.Lockout.MaxFailedAccessAttempts = 3;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.Run();

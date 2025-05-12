using Microsoft.EntityFrameworkCore;
using SDS.Data;
using SDS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<SdsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<SdsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Home}/{action=Index}/{id?}");

// Change the default route to SafetyDataSheet/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=SafetyDataSheet}/{action=Index}/{id?}");

// Add a redirect from the root to SafetyDataSheet/Index with page=1
app.MapGet("/", context =>
{
    context.Response.Redirect("/SafetyDataSheet/Index?page=1");
    return Task.CompletedTask;
});

app.Run();

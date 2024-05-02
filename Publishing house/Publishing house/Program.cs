using Microsoft.EntityFrameworkCore;
using Publishing_house.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Publishing_house.Data;
using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Publishing_house.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddDbContext<PublishingDBcontext>(options =>options.UseSqlServer(builder.Configuration.GetConnectionString("PublishingDB")));

//builder.Services.AddDefaultIdentity<Publishing_houseUser>(options => options.SignIn.RequireConfirmedAccount = true)
    //.AddEntityFrameworkStores<Publishing_houseContext>();

// Add services to the container.
builder.Services.AddDbContext<Publishing_houseContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Publishing_houseContextConnection")));
builder.Services.AddIdentity<Publishing_houseUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).
 AddEntityFrameworkStores<Publishing_houseContext>();
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.AccessDeniedPath = new PathString("/Identity/Account/AccessDenied");
    opt.LoginPath = new PathString("/Identity/Account/Login");
});


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
app.UseAuthentication();;

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebApplication2.Models;
using WebApplication2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<RazorpaySettings>(builder.Configuration.GetSection("Razorpay"));
builder.Services.AddSingleton<MongoDBService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Use detailed error page only in Development
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Use a generic error page or endpoint in Production
    app.UseHsts(); // Optionally enable HTTP Strict Transport Security (HSTS)
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

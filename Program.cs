using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelHierarchyApp.Models;
using ModelHierarchyApp.Services; // Adjust this if your namespace differs

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.Configure<OAuthSettings>(builder.Configuration.GetSection("OAuth"));
builder.Services.AddSingleton<OAuthService>(); 
builder.Services.AddTransient<ModelHierarchyService>();
builder.WebHost.UseUrls("http://localhost:3000");

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=OAuth}/{action=Login}/{id?}");
}); 

app.Run();

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelHierarchyApp.Models;
using ModelHierarchyApp.Services; // Adjust this if your namespace differs

/// <summary>
/// Entry point for the ModelHierarchyApp application.
/// Configures services and the HTTP request pipeline for the Razor Pages project.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

/// <summary>
/// Registers services required by the application into the dependency injection container.
/// </summary>
// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.Configure<OAuthSettings>(builder.Configuration.GetSection("OAuth"));
builder.Services.AddSingleton<OAuthService>();
builder.Services.AddHttpClient<GraphQLService>();
builder.Services.AddTransient<ModelHierarchyService>();
builder.WebHost.UseUrls("http://localhost:3000");

var app = builder.Build();

/// <summary>
/// Configures the HTTP request pipeline, including error handling, static files, routing, and session.
/// </summary>
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
    /// <summary>
    /// Configures the default route for MVC controllers.
    /// </summary>
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=OAuth}/{action=Login}/{id?}");
}); 

app.Run();

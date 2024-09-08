using Dashboard.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Register HttpClient for use in WeatherService
builder.Services.AddHttpClient();

// Register WeatherService
builder.Services.AddScoped<WeatherService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
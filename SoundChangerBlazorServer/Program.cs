using IgniteUI.Blazor.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using SoundChangerBlazorServer.Data;
using SoundChangerBlazorServer.Models.SpotifyModels;
using SoundChangerBlazorServer.Models.YoutubeModels;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SpotifyClientSettings>(builder.Configuration.GetSection(nameof(SpotifyClientSettings)));
builder.Services.Configure<YoutubeApiSettings>(builder.Configuration.GetSection(nameof(YoutubeApiSettings)));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<AudioService>();
builder.Services.AddSingleton<SpotifyClient>();
builder.Services.AddSingleton<YoutubeDownloader>();
builder.Services.AddIgniteUIBlazor(typeof(IgbSliderModule));
builder.Services.AddSyncfusionBlazor();


var clientSettings = builder.Configuration.GetSection(nameof(SpotifyClientSettings)).Get<SpotifyClientSettings>();

if (clientSettings != null)
{
    builder.Services.AddHttpClient(nameof(SpotifyClient), (client) =>
    {
        client.BaseAddress = new Uri(clientSettings.BaseUrl);
    });
    builder.Services.AddHttpClient("YoutubeClient", (client) =>
    {
        client.BaseAddress = new Uri("https://www.youtube.com/");
    });
}


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

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

var service = app.Services.GetRequiredService<AudioService>();
await service.Clear();

app.Run();

using IgniteUI.Blazor.Controls;
using SoundChangerBlazorServer.Models.SpotifyModels;
using SoundChangerBlazorServer.Models.YoutubeModels;
using SoundChangerBlazorServer.Services;
using SoundChangerBlazorServer.Services.Interfaces;
using SoundChangerBlazorServer.Utils;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SpotifyClientSettings>(builder.Configuration.GetSection(nameof(SpotifyClientSettings)));
builder.Services.Configure<YoutubeApiSettings>(builder.Configuration.GetSection(nameof(YoutubeApiSettings)));
builder.Services.Configure<YoutubeDownloaderSettings>(builder.Configuration.GetSection(nameof(YoutubeDownloaderSettings)));

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddYoutubeClient(builder.Configuration);
builder.Services.AddSingleton<IYoutubeDownloader, YoutubeDownloader>();
builder.Services.AddSingleton<IAudioService, AudioService>();
builder.Services.AddSingleton<SpotifyClient>();
builder.Services.AddSingleton<StateContainer>();
builder.Services.AddIgniteUIBlazor(typeof(IgbSliderModule));
builder.Services.AddSyncfusionBlazor();
builder.Services.AddSingleton<IClearService, ClearService>();

var app = builder.Build();

var service = app.Services.GetRequiredService<IClearService>();
await service.ClearFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

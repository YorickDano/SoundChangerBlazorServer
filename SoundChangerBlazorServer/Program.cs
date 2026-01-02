using IgniteUI.Blazor.Controls;
using SoundChangerBlazorServer.Middleware;
using SoundChangerBlazorServer.Models.GeniusModels;
using SoundChangerBlazorServer.Models.YoutubeModels;
using SoundChangerBlazorServer.Services;
using SoundChangerBlazorServer.Services.Interfaces;
using SoundChangerBlazorServer.Services.YoutubeServices;
using SoundChangerBlazorServer.Utils;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<YoutubeApiSettings>(builder.Configuration.GetSection(nameof(YoutubeApiSettings)));
builder.Services.Configure<YoutubeDownloaderSettings>(builder.Configuration.GetSection(nameof(YoutubeDownloaderSettings)));
builder.Services.Configure<GeniusSettings>(builder.Configuration.GetSection(nameof(GeniusSettings)));

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddLogging();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});


builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITokenStorageService, TokenStorageService>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<IYoutubeDownloader, YoutubeDownloader>();
builder.Services.AddSingleton<IAudioService, AudioService>();
builder.Services.AddSingleton<CutterService>();
builder.Services.AddSingleton<INextPageTokenService, YoutubeNextPageTokenService>();
builder.Services.AddScoped<GeniusService>();
builder.Services.AddSingleton<StateContainer>();
builder.Services.AddIgniteUIBlazor(typeof(IgbSliderModule));
builder.Services.AddSyncfusionBlazor();
builder.Services.AddSingleton<IClearService, ClearService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTelerikBlazor();
builder.Services.AddScoped<YoutubeMusicService>();
builder.Services.AddHttpContextAccessor();

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
app.UseMiddleware<UserMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.UseSession();

app.Run();

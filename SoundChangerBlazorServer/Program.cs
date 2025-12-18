using Google.Apis.Auth.AspNetCore3;
using IgniteUI.Blazor.Controls;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using SoundChangerBlazorServer.Models.GeniusModels;
using SoundChangerBlazorServer.Models.SpotifyModels;
using SoundChangerBlazorServer.Models.YoutubeModels;
using SoundChangerBlazorServer.Services;
using SoundChangerBlazorServer.Services.Interfaces;
using SoundChangerBlazorServer.Services.YoutubeServices;
using SoundChangerBlazorServer.Utils;
using Syncfusion.Blazor;
using Telerik.DataSource.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SpotifyClientSettings>(builder.Configuration.GetSection(nameof(SpotifyClientSettings)));
builder.Services.Configure<YoutubeApiSettings>(builder.Configuration.GetSection(nameof(YoutubeApiSettings)));
builder.Services.Configure<YoutubeDownloaderSettings>(builder.Configuration.GetSection(nameof(YoutubeDownloaderSettings)));
builder.Services.Configure<GeniusSettings>(builder.Configuration.GetSection(nameof(GeniusSettings)));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleOpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogleOpenIdConnect(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    options.Scope.AddRange(builder.Configuration.GetSection("Google:Scopes").Get<IEnumerable<string>>());
    options.SaveTokens = true;
});

builder.Services.AddAuthorization();

// Добавляем сервисы сессии
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddYoutubeClient(builder.Configuration);
builder.Services.AddScoped<CutterService>();
builder.Services.AddScoped<IYoutubeDownloader, YoutubeDownloader>();
builder.Services.AddScoped<IAudioService, AudioService>();
builder.Services.AddSingleton<INextPageTokenService, YoutubeNextPageTokenService>();
builder.Services.AddSingleton<SpotifyClient>();
builder.Services.AddScoped<GeniusService>();
builder.Services.AddSingleton<StateContainer>();
builder.Services.AddScoped<YoutubeMusicService>();
builder.Services.AddIgniteUIBlazor(typeof(IgbSliderModule));
builder.Services.AddSyncfusionBlazor();
builder.Services.AddSingleton<IClearService, ClearService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTelerikBlazor();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.UseSession();

app.Run();

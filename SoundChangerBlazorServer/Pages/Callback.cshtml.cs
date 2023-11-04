using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Converters;
using SoundChangerBlazorServer.Data;

namespace SoundChangerBlazorServer.Pages
{
    public class CallbackModel : PageModel
    {
        private readonly SpotifyClient SpotifyClient;

        public CallbackModel(SpotifyClient spotifyClient)
        {
            this.SpotifyClient = spotifyClient;
        }

        public async Task<IActionResult> OnGetAsync(string code)
        {
            await SpotifyClient.SetToken(code);

            return Page();
        }
    }
}

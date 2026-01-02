using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SoundChangerBlazorServer.Services;
using SoundChangerBlazorServer.Utils;

namespace SoundChangerBlazorServer.Pages
{
    public class CallbackModel : PageModel
    {
        private readonly GeniusService _geniusService;
        private readonly StateContainer _stateContainer;

        public CallbackModel(GeniusService geniusService, 
                             StateContainer stateContainer)
        {
            _geniusService = geniusService;
            _stateContainer = stateContainer;
        }

        public async Task<IActionResult> OnGetGeniusAsync(string code)
        {
            var token = await _geniusService.Authorize(code);
            _stateContainer.GeniusToken = token;

            return Page();
        }
    }
}

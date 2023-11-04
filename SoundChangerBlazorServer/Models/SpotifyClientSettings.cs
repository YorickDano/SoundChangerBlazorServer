﻿using Microsoft.Extensions.Options;

namespace SoundChangerBlazorServer.Models
{
    public class SpotifyClientSettings
    {
        public string BaseUrl { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}

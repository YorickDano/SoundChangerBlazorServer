namespace SoundChangerBlazorServer.Models.SpotifyModels
{
    public class SpotifyTrack
    {
        public class Album
        {
            public string album_type { get; set; }
            public int total_tracks { get; set; }
            public List<string> available_markets { get; set; }
            public ExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public List<Image> images { get; set; }
            public string name { get; set; }
            public string release_date { get; set; }
            public string release_date_precision { get; set; }
            public Restrictions restrictions { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public List<Artist> artists { get; set; }
        }

        public class Artist
        {
            public ExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public Followers followers { get; set; }
            public List<string> genres { get; set; }
            public List<Image> images { get; set; }
            public int popularity { get; set; }
        }

        public class ExternalIds
        {
            public string isrc { get; set; }
            public string ean { get; set; }
            public string upc { get; set; }
        }

        public class ExternalUrls
        {
            public string spotify { get; set; }
        }

        public class Followers
        {
            public string href { get; set; }
            public int total { get; set; }
        }

        public class Image
        {
            public string url { get; set; }
            public int height { get; set; }
            public int width { get; set; }
        }

        public class LinkedFrom
        {
        }

        public class Restrictions
        {
            public string reason { get; set; }
        }

        public class Root
        {
            public Album album { get; set; }
            public List<Artist> artists { get; set; }
            public List<string> available_markets { get; set; }
            public int disc_number { get; set; }
            public int duration_ms { get; set; }
            public bool @explicit { get; set; }
            public ExternalIds external_ids { get; set; }
            public ExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public bool is_playable { get; set; }
            public LinkedFrom linked_from { get; set; }
            public Restrictions restrictions { get; set; }
            public string name { get; set; }
            public int popularity { get; set; }
            public string preview_url { get; set; }
            public int track_number { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public bool is_local { get; set; }
        }


    }
}

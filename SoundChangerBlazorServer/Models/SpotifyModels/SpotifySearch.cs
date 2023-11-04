namespace SoundChangerBlazorServer.Models.SpotifyModels
{
    public class SpotifySearch
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
            public string type { get; set; }
            public string uri { get; set; }
            public List<Artist> artists { get; set; }
        }

        public class Albums
        {
            public string href { get; set; }
            public int limit { get; set; }
            public string next { get; set; }
            public int offset { get; set; }
            public object previous { get; set; }
            public int total { get; set; }
            public List<Item> items { get; set; }
        }

        public class Artist
        {
            public ExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public int limit { get; set; }
            public string next { get; set; }
            public int offset { get; set; }
            public object previous { get; set; }
            public int total { get; set; }
            public List<Item> items { get; set; }
        }

        public class Audiobooks
        {
            public string href { get; set; }
            public int limit { get; set; }
            public object next { get; set; }
            public int offset { get; set; }
            public object previous { get; set; }
            public int total { get; set; }
            public List<object> items { get; set; }
        }

        public class Episodes
        {
            public string href { get; set; }
            public int limit { get; set; }
            public object next { get; set; }
            public int offset { get; set; }
            public object previous { get; set; }
            public int total { get; set; }
            public List<object> items { get; set; }
        }

        public class ExternalIds
        {
            public string isrc { get; set; }
        }

        public class ExternalUrls
        {
            public string spotify { get; set; }
        }

        public class Followers
        {
            public object href { get; set; }
            public int total { get; set; }
        }

        public class Image
        {
            public string url { get; set; }
            public int? height { get; set; }
            public int? width { get; set; }
        }

        public class Item
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
            public string name { get; set; }
            public int popularity { get; set; }
            public string preview_url { get; set; }
            public int track_number { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public bool is_local { get; set; }
            public Followers followers { get; set; }
            public List<string> genres { get; set; }
            public List<Image> images { get; set; }
            public string album_type { get; set; }
            public int total_tracks { get; set; }
            public string release_date { get; set; }
            public string release_date_precision { get; set; }
            public bool collaborative { get; set; }
            public string description { get; set; }
            public Owner owner { get; set; }
            public object @public { get; set; }
            public string snapshot_id { get; set; }
            public Tracks tracks { get; set; }
            public object primary_color { get; set; }
        }

        public class Owner
        {
            public ExternalUrls external_urls { get; set; }
            public string href { get; set; }
            public string id { get; set; }
            public string type { get; set; }
            public string uri { get; set; }
            public string display_name { get; set; }
        }

        public class Playlists
        {
            public string href { get; set; }
            public int limit { get; set; }
            public string next { get; set; }
            public int offset { get; set; }
            public object previous { get; set; }
            public int total { get; set; }
            public List<Item> items { get; set; }
        }

        public class Root
        {
            public Tracks tracks { get; set; }
            public Artist artists { get; set; }
            public Albums albums { get; set; }
            public Playlists playlists { get; set; }
            public Shows shows { get; set; }
            public Episodes episodes { get; set; }
            public Audiobooks audiobooks { get; set; }
        }

        public class Shows
        {
            public string href { get; set; }
            public int limit { get; set; }
            public object next { get; set; }
            public int offset { get; set; }
            public object previous { get; set; }
            public int total { get; set; }
            public List<object> items { get; set; }
        }

        public class Tracks
        {
            public string href { get; set; }
            public int limit { get; set; }
            public string next { get; set; }
            public int offset { get; set; }
            public object previous { get; set; }
            public int total { get; set; }
            public List<Item> items { get; set; }
        }
    }
}

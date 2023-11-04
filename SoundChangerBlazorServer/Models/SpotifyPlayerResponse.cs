namespace SoundChangerBlazorServer.Models
{
    public class SpotifyPlayerResponse
    {
        public class Actions
        {
            public bool interrupting_playback { get; set; }
            public bool pausing { get; set; }
            public bool resuming { get; set; }
            public bool seeking { get; set; }
            public bool skipping_next { get; set; }
            public bool skipping_prev { get; set; }
            public bool toggling_repeat_context { get; set; }
            public bool toggling_shuffle { get; set; }
            public bool toggling_repeat_track { get; set; }
            public bool transferring_playback { get; set; }
        }

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
            public List<Copyright> copyrights { get; set; }
            public ExternalIds external_ids { get; set; }
            public List<string> genres { get; set; }
            public string label { get; set; }
            public int popularity { get; set; }
            public string album_group { get; set; }
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

        public class Context
        {
            public string type { get; set; }
            public string href { get; set; }
            public ExternalUrls external_urls { get; set; }
            public string uri { get; set; }
        }

        public class Copyright
        {
            public string text { get; set; }
            public string type { get; set; }
        }

        public class Device
        {
            public string id { get; set; }
            public bool is_active { get; set; }
            public bool is_private_session { get; set; }
            public bool is_restricted { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public int volume_percent { get; set; }
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

        public class Item
        {
            public Album album { get; set; }
            public List<Artist> artists { get; set; }
            public List<string> available_markets { get; set; }
            public long disc_number { get; set; }
            public long duration_ms { get; set; }
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

        public class LinkedFrom
        {
        }

        public class Restrictions
        {
            public string reason { get; set; }
        }

        public class Root
        {
            public Device device { get; set; }
            public string repeat_state { get; set; }
            public bool shuffle_state { get; set; }
            public Context context { get; set; }
            public long timestamp { get; set; }
            public long progress_ms { get; set; }
            public bool is_playing { get; set; }
            public Item item { get; set; }
            public string currently_playing_type { get; set; }
            public Actions actions { get; set; }
        }
    }
}

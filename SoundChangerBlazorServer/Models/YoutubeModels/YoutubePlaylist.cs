namespace SoundChangerBlazorServer.Models.YoutubeModels
{
    public class YoutubePlaylist
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string ImgUrl { get; set; }
        public IEnumerable<YoutubeVideo> Videos {get;set;}
}
}

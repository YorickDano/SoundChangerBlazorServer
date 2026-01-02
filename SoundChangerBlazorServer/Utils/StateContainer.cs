namespace SoundChangerBlazorServer.Utils
{
    public class StateContainer
    {
        public readonly Dictionary<int, object> ObjectTunnel = new();
        public readonly Dictionary<string, object> ObjectContainer = new();
        public string GeniusToken { get; set; }
    }
}

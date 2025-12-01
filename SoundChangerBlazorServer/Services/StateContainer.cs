namespace SoundChangerBlazorServer.Services
{
    public class StateContainer
    {
        public readonly Dictionary<int, object> ObjectTunnel = new();
        public string Token { get; set; }
    }
}

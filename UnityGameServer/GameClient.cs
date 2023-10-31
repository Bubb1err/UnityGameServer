namespace UnityGameServer
{
    public class GameClient
    {
        public Mutex mutex { get; private set; }
        public GameClient()
        {
            mutex = new Mutex();
        }
    }
}

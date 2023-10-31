namespace UnityGameServer.DataAccess.Entities
{
    public class Player
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string? ConnectionId { get; set; }
        public string GameId { get; set; }
        public Game Game { get; set; }
    }
}

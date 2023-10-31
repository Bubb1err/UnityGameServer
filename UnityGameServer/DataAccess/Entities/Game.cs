namespace UnityGameServer.DataAccess.Entities
{
    public class Game
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public List<Player> Players { get; set; } = new List<Player>();

        public bool InProgress { get; set; } = false;
        //public Game()
        //{
        //    Players = new List<Player>();
        //    InProgress = false;
        //}
        public Player? GetPlayer(string connectionId)
        {
            var player = Players.FirstOrDefault(p => p.ConnectionId == connectionId);
            return player;
        }
        public List<Player> GetUnassignedPlayers()
        {
            var players = Players.Where(p => p.ConnectionId == null).ToList();
            return players;
        }

    }
    public struct PlayerPosition
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}

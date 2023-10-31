using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UnityGameServer.DataAccess;
using UnityGameServer.DataAccess.Entities;
using UnityGameServer.DataAccess.Repository;

namespace UnityGameServer.Hubs
{
    public interface IGameClient
    {
        Task GameStart(string gameId);
        Task GameStop();
        Task Updated(string player);
    }

    public class GameHub : Hub<IGameClient>
    {
        private readonly IRepository<Game> _gameRepository;
        private readonly IRepository<Player> _playerRepository;
        private readonly ApplicationDbContext _context;
        private readonly GameClient _gameClient;
        private static int _maxPlayersCount = 4;
        public GameHub(IRepository<Game> gameRepository,
            IRepository<Player> playerRepository, ApplicationDbContext context, GameClient gameClient )
        {
            _gameRepository = gameRepository;
            _playerRepository = playerRepository;
            _context = context;
            _gameClient = gameClient;
        }
        public override async Task OnConnectedAsync()
        {
            try
            {
                _gameClient.mutex.WaitOne();

                var game = _context.Games.Include(g => g.Players).FirstOrDefault(g => !g.InProgress);
                if (game == null)
                {
                    game = new Game();
                    var player = new Player
                    {
                        ConnectionId = Context.ConnectionId,
                        GameId = game.Id,
                    };
                    game.Players.Add(player);
                    _context.Games.Add(game);
                    _context.Players.Add(player);
                    _context.SaveChanges();
                }
                else
                {
                    var player = new Player
                    {
                        ConnectionId = Context.ConnectionId,
                        GameId = game.Id
                    };
                    game.Players.Add(player);
                    _context.Players.Add(player);
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
                await base.OnConnectedAsync();
                if (game.Players.Count == _maxPlayersCount)
                {
                    game.InProgress = true;
                }
                _context.SaveChanges();
                if (game.InProgress)
                {
                   await Clients.Group(game.Id).GameStart(game.Id);                 
                }
                
                _gameClient.mutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }
        public async Task UpdatePlayerPosition(int x, int y)
        {
            var player = await _playerRepository.GetFirstOrDefaultAsync(p => p.ConnectionId == Context.ConnectionId);
            if (player != null)
            {
                player.X = x; player.Y = y;
                _playerRepository.Update(player);
                await _playerRepository.SaveAsync();
                await Clients.Group(player.GameId).Updated(JsonSerializer.Serialize<Player>(player));
            }
        }
    }

}

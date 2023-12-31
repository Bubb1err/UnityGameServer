﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UnityGameServer.DataAccess;
using UnityGameServer.DataAccess.Entities;
using UnityGameServer.DataAccess.Repository;

namespace UnityGameServer.Hubs
{
    public interface IGameClient
    {
        Task RoomFilled(string data);
        Task GameStop();
        Task Updated(string player);
        Task GameplayEventHandler(string type, string data, string dataType);
        Task Connected(int playerRoomId);
        Task PlayerConnected(int playerRoomId);
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
                int playerRoomId = -1;
                if (game == null)
                {
                    game = new Game();
                    var player = new Player
                    {
                        ConnectionId = Context.ConnectionId,
                        GameId = game.Id,
                        RoomId = game.Players.Count
                    };
                    playerRoomId = player.RoomId;
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
                        GameId = game.Id,
                        RoomId = game.Players.Count
                    };
                    playerRoomId = player.RoomId;
                    game.Players.Add(player);
                    _context.Players.Add(player);
                }
                await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
                await Clients.Client(Context.ConnectionId).Connected(playerRoomId);
                await Clients.Group(game.Id).PlayerConnected(playerRoomId);
                await base.OnConnectedAsync();
                if (game.Players.Count == _maxPlayersCount)
                {
                    game.InProgress = true;
                }
                _context.SaveChanges();
                if (game.InProgress)
                {
                    GameStartData gameStartData = 
                        new GameStartData(game.Id, game.Players
                            .Select(player => new PlayerInfoData {RoomId = player.RoomId, UserId = player.Id})
                            .ToList());
                   var dataToSend= JsonSerializer.Serialize<GameStartData>(gameStartData);
                   await Clients.Group(game.Id).RoomFilled(dataToSend);                 
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
        public async Task ServerGameplayEventHandler(string gameID,string type, string data, string dataType)
        {
            await Clients.Group(gameID).GameplayEventHandler(type, data, dataType);
        }

        public class GameStartData
        {
            public string GameID { get; set; }
            public List<PlayerInfoData> PlayersInfoData { get; set; }
            public GameStartData(string gameID, List<PlayerInfoData> playersInfoData) 
            {
                GameID = gameID;
                PlayersInfoData = playersInfoData;
            }
        }

        public class PlayerInfoData
        {
            public int UserId { get; set; }
            public int RoomId { get; set; }
        }
    }

}

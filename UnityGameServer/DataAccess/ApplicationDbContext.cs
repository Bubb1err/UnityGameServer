using Microsoft.EntityFrameworkCore;
using System;
using UnityGameServer.DataAccess.Entities;

namespace UnityGameServer.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }

    }
}

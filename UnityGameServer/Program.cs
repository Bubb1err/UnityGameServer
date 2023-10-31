using UnityGameServer.DataAccess;
using UnityGameServer.DataAccess.Repository;
using UnityGameServer.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using UnityGameServer.Hubs;
using UnityGameServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("InMemory"));
builder.Services.AddScoped<IRepository<Game>, Repository<Game>>();
builder.Services.AddScoped<IRepository<Player>, Repository<Player>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddSingleton<GameClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHub<GameHub>("/game");
app.MapControllers();

app.Run();

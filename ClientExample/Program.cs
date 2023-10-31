using Microsoft.AspNetCore.SignalR.Client;
class Program
{
    static async Task Main(string[] args)
    {
        string hubUrl = "http://localhost:5174/game";
        HubConnection connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

        connection.On<string>("GameStart", async action =>
            {
                Console.WriteLine($"Game started, game id {action}");

                await connection.InvokeAsync("UpdatePlayerPosition", new Random().Next(), new Random().Next());
            }
        );

        connection.On<string>("GameStop", action => Console.WriteLine("Game finished"));

        connection.On<string>("Updated", player => Console.WriteLine($"Player updated: {player}"));
        try
        {
            await connection.StartAsync();
            Console.WriteLine("Connection started!");
            Console.WriteLine("Waiting for other players...");

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while establishing connection: {ex.Message}");
        }
        finally
        {
            await connection.StopAsync();
        }
        Console.ReadLine();
    }
}
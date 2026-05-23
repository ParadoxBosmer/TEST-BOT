/*
 * Monster Hunt Bot - C# Template
 * Usage: dotnet run <server_url> <game_id> <bot_name>
 * See README.md for full API documentation
 */
using System.Text.Json;

namespace MonsterHuntBot;

class BotTemplate
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(5) };
    private readonly string _serverUrl;
    private readonly string _gameId;
    private readonly string _botName;
    private int? _playerId;

    public BotTemplate(string serverUrl, string gameId, string botName)
    {
        _serverUrl = serverUrl.TrimEnd('/');
        _gameId = gameId;
        _botName = botName;
    }

    public async Task<JsonElement?> GetGameState()
    {
        var url = $"{_serverUrl}/game/state/{_gameId}";
        var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(content);
    }

    public bool FindMyPlayerId(JsonElement gameState)
    {
        if (!gameState.TryGetProperty("Players", out var players)) return false;
        foreach (var player in players.EnumerateObject())
        {
            if (player.Value.GetProperty("Name").GetString() == _botName)
            {
                _playerId = player.Value.GetProperty("Id").GetInt32();
                return true;
            }
        }
        return false;
    }

    public bool IsMyTurn(JsonElement gameState)
    {
        if (!_playerId.HasValue || !gameState.TryGetProperty("Players", out var players)) return false;
        if (!players.TryGetProperty(_playerId.ToString(), out var myPlayer)) return false;
        var isFirst = myPlayer.GetProperty("First").GetBoolean();
        var gameStateStr = gameState.GetProperty("GameState").GetString();
        return (isFirst && gameStateStr == "Player1Turn") || (!isFirst && gameStateStr == "Player2Turn");
    }

    public bool IsGameOver(JsonElement gameState)
    {
        if (!gameState.TryGetProperty("GameState", out var state)) return false;
        return state.GetString() == "Ending";
    }

    static async Task Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: dotnet run <server_url> <game_id> <bot_name>");
            return;
        }

        var bot = new BotTemplate(args[0], args[1], args[2]);

        while (true)
        {
            var state = await bot.GetGameState();
            if (state.HasValue && bot.FindMyPlayerId(state.Value)) break;
            await Task.Delay(500);
        }

        Console.WriteLine($"Connected as Player {bot._playerId}\n");

        var state = await bot.GetGameState();
        while (state.HasValue && !bot.IsGameOver(state.Value))
        {
            if (bot.IsMyTurn(state.Value))
            {
                Console.WriteLine("My turn!");
                // TODO: Implement your strategy here
                // See README.md for available endpoints
                
                await Task.Delay(500);
                state = await bot.GetGameState();
            }
            else
            {
                await Task.Delay(500);
                state = await bot.GetGameState();
            }
        }
        Console.WriteLine($"Game {bot._gameId} has ended. Exiting...");
    }
}

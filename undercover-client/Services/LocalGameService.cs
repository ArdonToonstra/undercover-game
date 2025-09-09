using Microsoft.JSInterop;
using RoamingRoutes.Shared.Models.Games;
using System.Text.Json;

namespace RoamingRoutes.Client.Services;

public class LocalGameService : ILocalGameService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private System.Timers.Timer? _roundTimer;
    private const string StorageKey = "undercover_game_session";

    public GameStateDTO? CurrentGame { get; private set; }

    public LocalGameService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    // API-compatible: CreateGameAsync
    public async Task<GameStateDTO?> CreateGameAsync(CreateGameRequestDTO request)
    {
        var gs = await StartGameAsync(request.HostNickname, GameConstants.GameConfig.MaxPlayers);
        return gs;
    }

    // API-compatible: JoinGameAsync
    public async Task<GameStateDTO?> JoinGameAsync(string gameId, JoinGameRequestDTO request)
    {
        var result = await AddPlayerAsync(gameId, request.Nickname);
        if (result.Success)
        {
            return result.GameState;
        }
        return null;
    }

    public Task<GameStateDTO?> GetGameStateAsync(string gameId)
    {
        if (CurrentGame is not null && CurrentGame.GameId == gameId)
            return Task.FromResult<GameStateDTO?>(CurrentGame);
        return Task.FromResult<GameStateDTO?>(null);
    }

    public Task<List<GameStateDTO>> GetAvailableGamesAsync()
    {
        if (CurrentGame is not null)
            return Task.FromResult(new List<GameStateDTO> { CurrentGame });
        return Task.FromResult(new List<GameStateDTO>());
    }

    public Task<bool> CheckHealthAsync()
    {
        return Task.FromResult(true);
    }

    public Task<GameStateDTO> StartGameAsync(string hostNickname, int maxPlayers = 8)
    {
        var game = new GameStateDTO
        {
            GameId = Guid.NewGuid().ToString("N").Substring(0, 6),
            HostPlayerId = Guid.NewGuid().ToString("N"),
            Status = GameStatus.WaitingForPlayers,
            MaxPlayers = maxPlayers,
            CreatedAt = DateTime.UtcNow
        };

        var host = new PlayerDTO
        {
            Id = game.HostPlayerId,
            Nickname = hostNickname,
            IsHost = true,
            IsAlive = true
        };

        game.Players.Add(host);
        CurrentGame = game;

        return Task.FromResult(game);
    }

    public Task<PlayerInfoResponseDTO> AddPlayerAsync(string gameId, string nickname)
    {
        if (CurrentGame is null || CurrentGame.GameId != gameId)
        {
            throw new InvalidOperationException("Game not found");
        }

        if (CurrentGame.Players.Count >= CurrentGame.MaxPlayers)
        {
            return Task.FromResult(new PlayerInfoResponseDTO { Success = false, ErrorMessage = "Game is full" });
        }

        var playerId = Guid.NewGuid().ToString("N");
        var player = new MyPlayerInfoDTO
        {
            Id = playerId,
            Nickname = nickname,
            IsAlive = true,
            IsHost = false,
            Role = GameConstants.PlayerRoles.Civilian,
            Word = string.Empty
        };

        CurrentGame.Players.Add(player);

        return Task.FromResult(new PlayerInfoResponseDTO { Success = true, PlayerInfo = player, GameState = CurrentGame });
    }

    public Task<bool> AdvanceRoundAsync(string gameId)
    {
        if (CurrentGame is null || CurrentGame.GameId != gameId)
            return Task.FromResult(false);

        CurrentGame.CurrentRound++;
        CurrentGame.RoundStartTime = DateTime.UtcNow;
        CurrentGame.VotingPhase = false;
        CurrentGame.Status = GameStatus.InProgress;

        // Example: start a discussion timer
        StartRoundTimer(GameConstants.GameConfig.DiscussionTimeSeconds);

        return Task.FromResult(true);
    }

    public Task ResetAsync(string gameId)
    {
        if (CurrentGame is null || CurrentGame.GameId != gameId)
            return Task.CompletedTask;

        CurrentGame = null;
        StopRoundTimer();
        return Task.CompletedTask;
    }

    public async Task SaveToLocalAsync()
    {
        if (CurrentGame is null)
            return;

        var json = JsonSerializer.Serialize(CurrentGame);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    public async Task LoadFromLocalAsync()
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
            {
                CurrentGame = JsonSerializer.Deserialize<GameStateDTO>(json);
            }
        }
        catch
        {
            // ignore
        }
    }

    private void StartRoundTimer(int seconds)
    {
        StopRoundTimer();
        _roundTimer = new System.Timers.Timer(seconds * 1000);
        _roundTimer.AutoReset = false;
        _roundTimer.Elapsed += (s, e) =>
        {
            if (CurrentGame is not null)
            {
                CurrentGame.VotingPhase = true;
            }
        };
        _roundTimer.Start();
    }

    private void StopRoundTimer()
    {
        if (_roundTimer is not null)
        {
            _roundTimer.Stop();
            _roundTimer.Dispose();
            _roundTimer = null;
        }
    }

    public ValueTask DisposeAsync()
    {
        StopRoundTimer();
        return ValueTask.CompletedTask;
    }
}

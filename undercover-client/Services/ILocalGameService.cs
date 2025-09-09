using RoamingRoutes.Shared.Models.Games;

namespace RoamingRoutes.Client.Services;

public interface ILocalGameService
{
    GameStateDTO? CurrentGame { get; }

    // Compatibility methods matching the original client API
    Task<GameStateDTO?> CreateGameAsync(CreateGameRequestDTO request);
    Task<GameStateDTO?> JoinGameAsync(string gameId, JoinGameRequestDTO request);
    Task<GameStateDTO?> GetGameStateAsync(string gameId);
    Task<List<GameStateDTO>> GetAvailableGamesAsync();
    Task<bool> CheckHealthAsync();

    // Lower-level helpers
    Task<GameStateDTO> StartGameAsync(string hostNickname, int maxPlayers = 8);
    Task<PlayerInfoResponseDTO> AddPlayerAsync(string gameId, string nickname);
    Task<bool> AdvanceRoundAsync(string gameId);
    Task ResetAsync(string gameId);
    Task SaveToLocalAsync();
    Task LoadFromLocalAsync();
}

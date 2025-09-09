namespace RoamingRoutes.Shared.Models.Games
{
    /// <summary>
    /// Constants used throughout the game system
    /// </summary>
    public static class GameConstants
    {
        public static class PlayerRoles
        {
            public const string Civilian = "Civilian";
            public const string Undercover = "Undercover";
            public const string MrWhite = "MrWhite";
        }

        public static class GameConfig
        {
            public const int MinPlayers = 3;
            public const int MaxPlayers = 10;
            public const int DiscussionTimeSeconds = 300;
            public const int VotingTimeSeconds = 60;
            public const int ResultsTimeSeconds = 15;
            public const int RoomCodeLength = 6;
        }
    }
}

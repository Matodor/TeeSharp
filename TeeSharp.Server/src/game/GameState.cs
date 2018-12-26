namespace TeeSharp.Server.Game
{
    public enum GameState
    {
        /// <summary>
        /// Warmup started by game because there're not enough players (infinite)
        /// </summary>
        WarmupGame = 0,

        /// <summary>
        /// Warmup started by user action via rcon or new match (infinite or timer)
        /// </summary>
        WarmupUser,

        /// <summary>
        /// Start countown to unpause the game or start match/round (tick timer)
        /// </summary>
        StartCountdown,

        /// <summary>
        /// Game paused (infinite or tick timer)
        /// </summary>
        GamePaused,

        /// <summary>
        /// Game running (infinite)
        /// </summary>
        GameRunning,

        /// <summary>
        /// Match is over (tick timer)
        /// </summary>
        EndMatch,

        /// <summary>
        /// Round is over (tick timer)
        /// </summary>
        EndRound,
    }
}
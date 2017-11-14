using System;

namespace GridAiGames.Bomberman
{
    [Flags]
    public enum PlayerAction
    {
        None = 0,
        MoveLeft = 1 << 0,
        MoveUp = 1 << 1,
        MoveRight = 1 << 2,
        MoveDown = 1 << 3,

        /// <summary>
        /// Can be used together with Move commands.
        /// </summary>
        PlaceBomb = 1 << 4
    }
}

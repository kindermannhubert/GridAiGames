using System;

namespace GridAiGames.Bomberman
{
    [Flags]
    public enum PlayerAction
    {
        None,
        MoveLeft,
        MoveUp,
        MoveRight,
        MoveDown,
        PlaceBomb
    }
}

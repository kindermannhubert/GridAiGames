using System.Collections.Generic;

namespace GridAiGames
{
    public interface IReadOnlyGameGrid<PlayerType, PlayerActionType>
        where PlayerType : IPlayer<PlayerType, PlayerActionType>
    {
        int Width { get; }
        int Height { get; }

        IEnumerable<GameObject<PlayerType, PlayerActionType>> AllObjects { get; }
        IEnumerable<PlayerType> AllPlayers { get; }

        IReadOnlyList<GameObject<PlayerType, PlayerActionType>> GetObjects(int x, int y);
        IReadOnlyList<PlayerType> GetPlayers(int x, int y);
    }

    public static class IReadOnlyGameGridX
    {
        public static IReadOnlyList<GameObject<PlayerType, PlayerActionType>> GetObjects<PlayerType, PlayerActionType>(this IReadOnlyGameGrid<PlayerType, PlayerActionType> grid, Position position)
            where PlayerType : Player<PlayerType, PlayerActionType>
            => grid.GetObjects(position.X, position.Y);

        public static IReadOnlyList<GameObject<PlayerType, PlayerActionType>> GetPlayers<PlayerType, PlayerActionType>(this IReadOnlyGameGrid<PlayerType, PlayerActionType> grid, Position position)
            where PlayerType : Player<PlayerType, PlayerActionType>
            => grid.GetObjects(position.X, position.Y);
    }
}
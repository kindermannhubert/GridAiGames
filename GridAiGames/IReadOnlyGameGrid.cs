using System.Collections.Generic;

namespace GridAiGames
{
    public interface IReadOnlyGameGrid
    {
        int Width { get; }
        int Height { get; }

        IEnumerable<IReadOnlyGameObject> AllObjects { get; }
        IEnumerable<IPlayer> AllPlayers { get; }

        IReadOnlyList<IReadOnlyGameObject> GetObjects(int x, int y);
        IReadOnlyList<IPlayer> GetPlayers(int x, int y);
    }

    public static class IReadOnlyGameGridX
    {
        public static IReadOnlyList<IReadOnlyGameObject> GetObjects(this IReadOnlyGameGrid grid, Position position)
            => grid.GetObjects(position.X, position.Y);

        public static IReadOnlyList<IPlayer> GetPlayers(this IReadOnlyGameGrid grid, Position position)
            => grid.GetPlayers(position.X, position.Y);
    }
}
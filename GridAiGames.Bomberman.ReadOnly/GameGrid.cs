using System.Collections.Generic;

namespace GridAiGames.Bomberman.ReadOnly
{
    public class GameGrid
    {
        private readonly IReadOnlyList<ReadOnlyGameObject>[,] allObjects;

        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<Player> AllPlayers { get; }

        public IEnumerable<ReadOnlyGameObject> AllObjects
        {
            get
            {
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        foreach (var obj in allObjects[x, y])
                        {
                            yield return obj;
                        }
            }
        }

        public IReadOnlyList<ReadOnlyGameObject> GetObjects(Position position) => GetObjects(position.X, position.Y);
        public IReadOnlyList<ReadOnlyGameObject> GetObjects(int x, int y) => allObjects[x, y];

        public GameGrid(IReadOnlyList<ReadOnlyGameObject>[,] allObjects, int width, int height, IReadOnlyList<Player> allPlayers)
        {
            this.allObjects = allObjects;
            Width = width;
            Height = height;
            AllPlayers = allPlayers;
        }
    }
}

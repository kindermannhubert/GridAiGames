using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GridAiGames.Bomberman.ReadOnly
{
    public class GameGrid
    {
        private readonly IReadOnlyList<IReadOnlyGameObject>[,] allObjects;
        private readonly List<IWall> allWalls;
        private readonly List<IBomb> allBombs;
        private readonly List<IBonus> allBonuses;

        private readonly IWall[,] walls;
        private readonly IBomb[,] bombs;
        private readonly IBonus[,] bonuses;

        public int Width { get; }
        public int Height { get; }

        public IReadOnlyList<IPlayer> Players { get; private set; }
        public IReadOnlyList<IWall> Walls => allWalls;
        public IReadOnlyList<IBomb> Bombs => allBombs;
        public IReadOnlyList<IBonus> Bonuses => allBonuses;

        public IEnumerable<IReadOnlyGameObject> Objects
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

        public GameGrid(int width, int height)
        {
            Width = width;
            Height = height;

            allObjects = new IReadOnlyList<ReadOnlyGameObject>[width, height];

            allWalls = new List<IWall>(width * height);
            allBombs = new List<IBomb>(width * height);
            allBonuses = new List<IBonus>(width * height);

            walls = new Wall[width, height];
            bombs = new Bomb[width, height];
            bonuses = new Bonus[width, height];
        }

        public IReadOnlyList<IReadOnlyGameObject> GetObjects(Position position) => GetObjects(position.X, position.Y);
        public IReadOnlyList<IReadOnlyGameObject> GetObjects(int x, int y) => allObjects[x, y];

        /// <summary>
        /// Returns null if there is no wall.
        /// </summary>
        public IWall GetWall(Position position) => GetWall(position.X, position.Y);

        /// <summary>
        /// Returns null if there is no wall.
        /// </summary>
        public IWall GetWall(int x, int y) => walls[x, y];

        /// <summary>
        /// Returns null if there is no bomb.
        /// </summary>
        public IBomb GetBomb(Position position) => GetBomb(position.X, position.Y);

        /// <summary>
        /// Returns null if there is no bomb.
        /// </summary>
        public IBomb GetBomb(int x, int y) => bombs[x, y];

        /// <summary>
        /// Returns null if there is no bonus.
        /// </summary>
        public IBonus GetBonus(Position position) => GetBonus(position.X, position.Y);

        /// <summary>
        /// Returns null if there is no bonus.
        /// </summary>
        public IBonus GetBonus(int x, int y) => bonuses[x, y];

        internal void SetUp(IReadOnlyList<IReadOnlyGameObject>[,] allObjects, IReadOnlyList<IPlayer> allPlayers)
        {
            Players = allPlayers;

            Array.Clear(walls, 0, Width * Height);
            Array.Clear(bombs, 0, Width * Height);
            Array.Clear(bonuses, 0, Width * Height);

            allWalls.Clear();
            allBombs.Clear();
            allBonuses.Clear();

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    this.allObjects[x, y] = allObjects[x, y];

                    foreach (var obj in allObjects[x, y])
                    {
                        switch (obj)
                        {
                            case Wall w:
                                Debug.Assert(walls[x, y] == null);
                                walls[x, y] = w;
                                allWalls.Add(w);
                                break;
                            case Bomb b:
                                Debug.Assert(bombs[x, y] == null);
                                bombs[x, y] = b;
                                allBombs.Add(b);
                                break;
                            case Bonus b:
                                Debug.Assert(bonuses[x, y] == null);
                                bonuses[x, y] = b;
                                allBonuses.Add(b);
                                break;
                            default:
                                throw new InvalidOperationException($"Unknown object type: {obj.GetType()}.");
                        }
                    }
                }
        }
    }
}

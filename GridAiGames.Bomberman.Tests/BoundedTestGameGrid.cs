using System;
using System.Collections.Generic;
using GridAiGames.Logging;

namespace GridAiGames.Bomberman.Tests
{
    internal class BoundedTestGameGrid : TestGameGrid
    {
        public BoundedTestGameGrid(
            int width,
            int height,
            IReadOnlyList<TeamDefinition<ReadOnly.GameGrid, ReadOnly.IPlayer, PlayerAction>> teamDefinitions,
            Dictionary<string, Position> playerPositionsPerName,
            Action<GameGrid> addGameObjects,
            ILogger logger = null)
            : base(width,
                   height,
                   teamDefinitions,
                   playerPositionsPerName,
                   grid =>
                   {
                       AddBounds(grid);
                       addGameObjects(grid);
                   },
                   logger)
        {
        }

        private static void AddBounds(GameGrid grid)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                grid.AddObject(new Wall(new Position(x, 0)));
                grid.AddObject(new Wall(new Position(x, grid.Height - 1)));
            }
            for (int y = 1; y < grid.Height - 1; y++)
            {
                grid.AddObject(new Wall(new Position(0, y)));
                grid.AddObject(new Wall(new Position(grid.Width - 1, y)));
            }
        }
    }
}

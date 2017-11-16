using System;
using System.Collections.Generic;
using GridAiGames.Logging;

namespace GridAiGames.Bomberman.Tests
{
    internal class TestGameGrid : GameGrid
    {
        private ulong iteration;

        public TestGameGrid(
            int width,
            int height,
            IReadOnlyList<TeamDefinition<ReadOnly.GameGrid, ReadOnly.Player, PlayerAction>> teamDefinitions,
            Dictionary<string, Position> playerPositionsPerName,
            Action<GameGrid> addGameObjects,
            ILogger logger = null)
            : base(
                  width,
                  height,
                  teamDefinitions,
                  (teamName, playerName) => playerPositionsPerName[playerName],
                  addGameObjects,
                  new Random(1),
                  logger ?? new TestLogger(canGenerateWarningAndErrors: false))
        {
        }

        public void Update()
        {
            Update(iteration++);
        }
    }
}

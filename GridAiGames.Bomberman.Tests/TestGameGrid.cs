using System;
using System.Collections.Generic;

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
            Action<GameGrid> addGameObjects)
            : base(
                  width,
                  height,
                  teamDefinitions,
                  (teamName, playerName) => playerPositionsPerName[playerName],
                  addGameObjects,
                  new Random(1),
                  new TestLogger())
        {
        }

        public void Update()
        {
            Update(iteration++);
        }
    }
}
